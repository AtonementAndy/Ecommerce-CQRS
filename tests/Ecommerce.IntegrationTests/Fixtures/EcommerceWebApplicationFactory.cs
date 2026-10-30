using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Common.OutBox;
using Ecommerce.Application.Dtos;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

namespace Ecommerce.IntegrationTests.Fixtures;

public sealed class EcommerceWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string JwtIssuer = "https://tests.example";
    public const string JwtAudience = "shop-api";

    // Test-only key; never use this value outside isolated tests.
    public const string JwtKey = "test-only-signing-key-never-use-in-production-123456";

    public FakeCatalogStore Store { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:Issuer", JwtIssuer);
        builder.UseSetting("Jwt:Audience", JwtAudience);
        builder.UseSetting("Jwt:Key", JwtKey);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.RemoveAll<IDistributedCache>();
            services.AddDistributedMemoryCache();

            services.RemoveAll<IProductWriteRepository>();
            services.RemoveAll<IProductReadRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.RemoveAll<IOutboxWriter>();

            services.AddSingleton(Store);
            services.AddSingleton<IProductWriteRepository>(Store);
            services.AddSingleton<IProductReadRepository>(Store);
            services.AddSingleton<IUnitOfWork>(Store);
            services.AddSingleton<IOutboxWriter>(Store);
        });
    }

    public sealed class FakeCatalogStore :
        IProductWriteRepository,
        IProductReadRepository,
        IUnitOfWork,
        IOutboxWriter
    {
        private readonly ConcurrentDictionary<Guid, Product> _products = new();

        public void Reset() => _products.Clear();

        public Task<Product> AddAsync(Product product, CancellationToken cancellationToken)
        {
            if (!_products.TryAdd(product.Id, product))
                throw new InvalidOperationException("The product already exists.");

            return Task.FromResult(product);
        }

        public Task<Product?> UpdateAsync(
            Guid id,
            string name,
            string description,
            decimal price,
            int stock,
            int expectedVersion,
            CancellationToken cancellationToken)
        {
            if (!_products.TryGetValue(id, out var product))
                return Task.FromResult<Product?>(null);

            if (product.Version != expectedVersion)
                throw new DbUpdateConcurrencyException();

            product.UpdateDetails(name, description, price, stock);
            return Task.FromResult<Product?>(product);
        }

        public Task<bool> DeleteAsync(Guid id, int version, CancellationToken cancellationToken)
        {
            if (!_products.TryGetValue(id, out var product))
                return Task.FromResult(false);

            if (product.Version != version)
                throw new DbUpdateConcurrencyException();

            return Task.FromResult(_products.TryRemove(id, out _));
        }

        public Task<(IEnumerable<ProductReadModel> items, long totalCount)> GetAllAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var items = _products.Values
                .OrderBy(product => product.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(ToReadModel)
                .ToArray();

            return Task.FromResult(((IEnumerable<ProductReadModel>)items, (long)_products.Count));
        }

        public Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var result = _products.TryGetValue(id, out var product) ? ToReadModel(product) : null;
            return Task.FromResult(result);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(1);

        public Task EnqueueAsync<TEvent>(
            string aggregateType,
            TEvent @event,
            CancellationToken cancellationToken) where TEvent : class
            => Task.CompletedTask;

        private static ProductReadModel ToReadModel(Product product) => new()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Version = product.Version
        };
    }
}

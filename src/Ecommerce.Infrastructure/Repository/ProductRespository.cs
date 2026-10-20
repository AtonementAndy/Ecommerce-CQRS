using Ecommerce.Application.Common.OutBox;
using Ecommerce.Application.Dtos;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Data;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Polly;

namespace Ecommerce.Infrastructure.Repository
{
    internal class ProductRespository : IProductRepository
    {
        private readonly EcommerceDbContext _context;
        private readonly IMongoCollection<ProductReadModel> _mongoCollection;

        private static readonly AsyncPolicy _concurrencyRetry = Policy
            .Handle<DbUpdateConcurrencyException>()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(50 * attempt));

        public ProductRespository(EcommerceDbContext context, IMongoDatabase mongoDatabase, IConfiguration configuration)
        {
            _context = context;
            var collectionName = configuration["MongoDbSettings:CollectionName"] ?? "ProductsReadModel";
            _mongoCollection = mongoDatabase.GetCollection<ProductReadModel>(collectionName);
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            // 1. Busca pelo Guid diretamente graças ao [BsonRepresentation(BsonType.String)] do DTO
            var filter = Builders<ProductReadModel>.Filter.And(
                Builders<ProductReadModel>.Filter.Eq(p => p.Id, id),
                Builders<ProductReadModel>.Filter.Eq(p => p.IsDeleted, false)
            );
                            ;
            var doc = await _mongoCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);

            if (doc is null)
                return null;

            // 2. Reconstitui o produto preservando EXATAMENTE o Id e o CreatedAt originais do Mongo
            return new Product(
                doc.Id,
                doc.Name,
                doc.Description,
                doc.Price,
                doc.Stock,
                doc.CreatedAt,
                doc.UpdatedAt,
                doc.Version
            );
        }
        public async Task<(IEnumerable<Product> items, long totalCount)> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            var filter = Builders<ProductReadModel>.Filter.Empty;

            var totalCount = await _mongoCollection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

            var docs = await _mongoCollection.Find(filter)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            var items = docs.Select(doc => new Product(
                doc.Id,
                doc.Name,
                doc.Description,
                doc.Price,
                doc.Stock,
                doc.CreatedAt,
                doc.UpdatedAt,
                doc.Version
            ));

            return (items, totalCount);
        }

        public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken)
        {
            var productExists = await ExistsByNameAsync(product.Name, cancellationToken);
            if (productExists)
                throw new InvalidOperationException($"There is already a registered product with the name '{product.Name}'.");

            await _context.Products.AddAsync(product, cancellationToken);

            return product;
        }

        public async Task<Product?> UpdateAsync(Guid id, string name, string description, decimal price, int stock, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FindAsync([id], cancellationToken);
            if (product is null) return null;

            product.UpdateDetails(
                name,
                description,
                price,
                stock
            );

            return product;
        }

        public async Task<bool> DeleteAsync(Guid id, int expextedVersion, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FindAsync([id], cancellationToken);
            if (product is null) return false;

            _context.Entry(product).Property(p => p.Version).OriginalValue = expextedVersion;

            _context.Products.Remove(product);

            return true;
        }

        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
        {
            return await _context.Products
                .AnyAsync(p => p.Name.ToLower() == name.ToLower(), cancellationToken);
        }

        public Task EnqueueAsync<TEvent>(EcommerceDbContext context, string aggregateType, TEvent @event, CancellationToken cancellationToken) where TEvent : class
        {
            throw new NotImplementedException();
        }

        public Task AddWithOutboxAsync<TEvent>(Product product, string aggregateType, TEvent @event, IOutboxWriter outboxWriter, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
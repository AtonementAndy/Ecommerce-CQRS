using Ecommerce.Application.Common.Cache;
using Ecommerce.Application.Common.Events;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Ecommerce.Application.Products.Commands.CreateProduct
{
    public sealed class CreateProductCommandHandler(
        IProductRepository repository, 
        IPublishEndpoint publishEndpoint, 
        IDistributedCache cache) : IRequestHandler<CreateProductCommand, Guid>
    {
        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var productExists = await repository.ExistsByNameAsync(request.Name, cancellationToken);
            if (productExists)
            {
                throw new InvalidOperationException($"There is already a registered product with the name '{request.Name}'.");
            }

            var product = new Product(
                request.Name, 
                request.Description, 
                request.Price, 
                request.Stock
            );

            await repository.AddAsync(product, cancellationToken);

            var eventMessage = new ProductCreatedEvent(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Stock,
                product.CreatedAt
            );

            var cacheKey = CacheKeys.GetProductByIdKey(product.Id);
            var cacheKeyPaginated = CacheKeys.GetProductsKey(1, 10);

            await cache.SetStringAsync(
                cacheKey, 
                JsonSerializer.Serialize(eventMessage), 
                CacheOptions.DefaultExpiration, 
                cancellationToken
            );

            //await cache.RemoveAsync(cacheKey, cancellationToken);
            await cache.RemoveAsync(cacheKeyPaginated, cancellationToken);

            await publishEndpoint.Publish(eventMessage, cancellationToken);

            return product.Id;
        }
    }
}

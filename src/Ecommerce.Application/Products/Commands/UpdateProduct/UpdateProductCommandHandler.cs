using Ecommerce.Application.Common.Cache;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Messaging.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Ecommerce.Application.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler(
        IProductWriteRepository repository, 
        IDistributedCache cache, 
        IPublishEndpoint publishEndpoint) : IRequestHandler<UpdateProductCommand, Guid>
    {
        public async Task<Guid> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await repository.GetByIdAsync(request.Id, cancellationToken) 
                ?? throw new ArgumentNullException($"Product with ID {request.Id} not found.");

            var updatedProductDetails = new Product(
                request.Id,
                request.Name,
                request.Description,
                request.Price,
                request.Stock,
                product.CreatedAt
            );

            var sucess = await repository.UpdateASync(request.Id, updatedProductDetails, cancellationToken);
            if(!sucess)
                throw new InvalidOperationException($"Could not update product with ID {request.Id}.");

            var eventMessage = new ProductUpdatedEvent(
                updatedProductDetails.Id,
                updatedProductDetails.Name,
                updatedProductDetails.Description,
                updatedProductDetails.Price,
                updatedProductDetails.Stock,
                updatedProductDetails.CreatedAt
            );

            await publishEndpoint.Publish(eventMessage, cancellationToken);

            var cacheKey = CacheKeys.GetProductByIdKey(request.Id);
            var cacheKeyPaginated = CacheKeys.GetProductsKey(1, 10);

            await cache.RemoveAsync(cacheKey, cancellationToken);
            await cache.RemoveAsync(cacheKeyPaginated, cancellationToken);

            return product.Id;
        }
    }
}

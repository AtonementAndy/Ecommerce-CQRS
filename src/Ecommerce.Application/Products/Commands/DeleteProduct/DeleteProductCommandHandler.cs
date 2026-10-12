using Ecommerce.Application.Common.Cache;
using Ecommerce.Application.Common.Events;
using Ecommerce.Domain.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Ecommerce.Application.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandHandler(
        IProductWriteRepository repository, 
        IDistributedCache cache, 
        IPublishEndpoint publishEndpoint) : IRequestHandler<DeleteProductCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await repository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Product with ID '{request.Id}' was not found.");

            await repository.DeleteAsync(product.Id, cancellationToken);

            var eventMessage = new ProductDeletedEvent(product.Id);
            await publishEndpoint.Publish(eventMessage, cancellationToken);

            var cacheKey = CacheKeys.GetProductByIdKey(product.Id);
            var cacheKeyPaginated = CacheKeys.GetProductsKey(1, 10);

            await cache.RemoveAsync(cacheKey, cancellationToken);
            await cache.RemoveAsync(cacheKeyPaginated, cancellationToken);

            return Unit.Value;

        }
    }
}

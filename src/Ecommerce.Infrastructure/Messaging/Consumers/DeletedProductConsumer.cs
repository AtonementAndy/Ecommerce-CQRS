using DnsClient.Internal;
using Ecommerce.Application.Common.Cache;
using Ecommerce.Application.Common.Events;
using Ecommerce.Application.Dtos;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Ecommerce.Infrastructure.Messaging.Consumers
{
    public class DeletedProductConsumer(ILogger<DeletedProductConsumer> logger, IMongoDatabase mongoDatabase, IDistributedCache cache) : IConsumer<ProductDeletedEvent>
    {
        private readonly IMongoCollection<ProductReadModel> _mongoCollection = mongoDatabase.GetCollection<ProductReadModel>("ProductsReadModel");

        public async Task Consume(ConsumeContext<ProductDeletedEvent> context)
        {
            await _mongoCollection.DeleteOneAsync(
                p => p.Id == context.Message.Id,
                cancellationToken: context.CancellationToken
            );

            var cacheKey = CacheKeys.GetProductsKey(1, 10);

            await cache.RemoveAsync(cacheKey, context.CancellationToken);

            logger.LogInformation($"[RabbitMQ] Read DB document removed! (ID: {context.Message.Id})");
        }
    }
}

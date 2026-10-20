using DnsClient.Internal;
using Ecommerce.Application.Common.Cache;
using Ecommerce.Application.Dtos;
using Ecommerce.Infrastructure.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Ecommerce.Infrastructure.Messaging.Consumers
{
    public class UpdatedProductConsumer(
        ILogger<UpdatedProductConsumer> logger, 
        IMongoDatabase mongoDatabase,
        IDistributedCache cache) : IConsumer<ProductUpdatedEvent>
    {
        private readonly IMongoCollection<ProductReadModel> _mongoCollection = mongoDatabase.GetCollection<ProductReadModel>("ProductsReadModel");
        public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
        {
            var message = context.Message;

            var filter = Builders<ProductReadModel>.Filter.Eq(p => p.Id, message.Id);

            var existing = await _mongoCollection
                .Find(filter)
                .FirstOrDefaultAsync(context.CancellationToken);

            if(existing is not null && message.Version <= existing.Version)
            {
                logger.LogInformation($"[RabbitMQ] Event ignored (duplicated/old). Id={message.Id}, EventVersion={message.Version}, CurrentVersion={existing.Version}");
                return;
            }

            if(existing is not null && message.Version > existing.Version + 1)
            {
                logger.LogWarning($"[RabbitMQ] Version gap detected. Id={message.Id}, EventVersion={message.Version}, CurrentVersion={existing.Version}");
            }

            var update = Builders<ProductReadModel>.Update
                .Set(p => p.Name, message.Name)
                .Set(p => p.Description, message.Description)
                .Set(p => p.Price, message.Price)
                .Set(p => p.Stock, message.Stock)
                .Set(p => p.UpdatedAt, message.UpdatedAt)
                .Set(p => p.Version, message.Version)
                .SetOnInsert(p => p.CreatedAt, message.UpdatedAt);

            await _mongoCollection.UpdateOneAsync(
                filter: filter,
                update: update,
                options: new UpdateOptions { IsUpsert = true },
                cancellationToken: context.CancellationToken
            );

            await CacheInvalidator.InvalidateProductCachesAsync(cache, message.Id, context.CancellationToken);

            logger.LogInformation($"[RabbitMQ] Event received! Product updated: {message.Name}, ID: {message.Id}, Version: {message.Version}");
        }
    }
}

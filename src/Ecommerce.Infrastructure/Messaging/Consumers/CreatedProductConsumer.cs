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
    public class CreatedProductConsumer(
        ILogger<CreatedProductConsumer> logger, 
        IMongoDatabase mongoDatabase,
        IDistributedCache cache) : IConsumer<ProductCreatedEvent>
    {
        private readonly IMongoCollection<ProductReadModel> _mongoCollection = mongoDatabase.GetCollection<ProductReadModel>("ProductsReadModel");

        public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
        {
            var message = context.Message;

            var filter = Builders<ProductReadModel>.Filter.Eq(p => p.Id, message.Id);

            var existing = await _mongoCollection
                .Find(filter)
                .FirstOrDefaultAsync(context.CancellationToken);

            if (existing is not null && message.Version <= existing.Version)
            {
                logger.LogInformation(
                    "[RabbitMQ] Created ignoraded (duplicated). Id={Id}, Version={V}",
                    message.Id, message.Version);
                return;
            }

            var readModel = new ProductReadModel
            {
                Id = message.Id,
                Name = message.Name,
                Description = message.Description,
                Price = message.Price,
                Stock = message.Stock,
                CreatedAt = message.CreatedAt,
                UpdatedAt = message.CreatedAt,
                Version = message.Version
            };

            await _mongoCollection.ReplaceOneAsync(
                filter: filter,
                replacement: readModel,
                options: new ReplaceOptions { IsUpsert = true },
                cancellationToken: context.CancellationToken
            );

            await CacheInvalidator.InvalidateProductCachesAsync(cache, message.Id, context.CancellationToken);

            logger.LogInformation($"[RabbitMQ] Event received! Product created: {message.Name} (ID: {message.Id})");
        }
    }
}

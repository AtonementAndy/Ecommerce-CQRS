using DnsClient.Internal;
using Ecommerce.Application.Dtos;
using Ecommerce.Infrastructure.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Ecommerce.Infrastructure.Messaging.Consumers
{
    public class UpdatedProductConsumer(ILogger<UpdatedProductConsumer> logger, IMongoDatabase mongoDatabase) : IConsumer<ProductUpdatedEvent>
    {
        private readonly IMongoCollection<ProductReadModel> _mongoCollection = mongoDatabase.GetCollection<ProductReadModel>("ProductsReadModel");
        public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
        {
            var message = context.Message;

            var productReadModel = new ProductReadModel
            {
                Id = message.Id,
                Name = message.Name,
                Description = message.Description,
                Price = message.Price,
                Stock = message.Stock,
                CreatedAt = message.CreatedAt
            };

            await _mongoCollection.ReplaceOneAsync(
                filter: p => p.Id == message.Id,
                replacement: productReadModel,
                options: new ReplaceOptions { IsUpsert = true },
                cancellationToken: context.CancellationToken
            );

            logger.LogInformation($"[RabbitMQ] Event received! Product updated: {message.Name} (ID: {message.Id})");
        }
    }
}

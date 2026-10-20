using Ecommerce.Application.Common.OutBox;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Data;
using System.Text.Json;

namespace Ecommerce.Infrastructure.Outbox
{
    public class OutboxWriter(EcommerceDbContext context) : IOutboxWriter
    {
        public Task EnqueueAsync<TEvent>(
            string aggregateType, 
            TEvent eventOutbox, 
            CancellationToken cancellationToken) where TEvent : class
        {
            var eventType = OutboxEventRegistry.NameOf<TEvent>();
            var payload = JsonSerializer.Serialize(eventOutbox);

            var message = OutboxMessage.Create(aggregateType, eventType, payload);
            context.Outbox.Add(message);

            return Task.CompletedTask;
        }
    }
}

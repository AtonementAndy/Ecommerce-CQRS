using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities
{
    public class OutboxMessage
    {
        public Guid Id { get; private set; }
        public string AggregateType { get; private set; } = string.Empty;
        public string EventType { get; private set; } = string.Empty;
        public string Payload { get; private set; } = string.Empty;
        public OutboxStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public string? Error { get; private set; }
        public int RetryCount { get; private set; }

        private OutboxMessage() { } 

        public static OutboxMessage Create(
            string aggregateType,
            string eventType,
            string payload)
        {
            return new OutboxMessage
            {
                Id = Guid.NewGuid(),
                AggregateType = aggregateType,
                EventType = eventType,
                Payload = payload,
                Status = OutboxStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                RetryCount = 0
            };
        }

        public void MarkAsPublished()
        {
            Status = OutboxStatus.Published;
            ProcessedAt = DateTime.UtcNow;
            Error = null;
        }

        public void MarkAsFailed(string error)
        {
            Status = OutboxStatus.Failed;
            Error = error.Length > 2000 ? error[..2000] : error;
            RetryCount++;
        }

        public void MarkAsProcessing()
        {
            Status = OutboxStatus.Processing;
            ProcessedAt = DateTime.UtcNow;
            Error = null;
        }
    }
}
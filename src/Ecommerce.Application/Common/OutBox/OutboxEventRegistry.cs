using Ecommerce.Application.Common.Events;

namespace Ecommerce.Application.Common.OutBox
{
    public static class OutboxEventRegistry
    {
        private static readonly Dictionary<string, Type> Map = new()
        {
            ["ProductCreated"] = typeof(ProductCreatedEvent),
            ["ProductUpdated"] = typeof(ProductUpdatedEvent),
            ["ProductDeleted"] = typeof(ProductDeletedEvent),
        };

        private static readonly Dictionary<Type, string> Reverse =
            Map.ToDictionary(kv => kv.Value, kv => kv.Key);

        public static Type Resolve(string eventType) =>
            Map.TryGetValue(eventType, out var t) ? t 
            : throw new InvalidOperationException($"Event type '{eventType}' unknown.");

        public static string NameOf<TEvent>() where TEvent : class =>
            Reverse.TryGetValue(typeof(TEvent), out var name)
            ? name
            : throw new InvalidOperationException($"Event {typeof(TEvent).Name} not registered.");
    }
}

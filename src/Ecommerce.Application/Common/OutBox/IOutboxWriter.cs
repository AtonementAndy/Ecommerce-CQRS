namespace Ecommerce.Application.Common.OutBox
{
    public interface IOutboxWriter
    {
        Task EnqueueAsync<TEvent>(
            string aggregateType,
            TEvent @event,
            CancellationToken cancellationToken) where TEvent : class;
    }
}

namespace Ecommerce.Domain.Enums
{
    public enum OutboxStatus
    {
        Pending,
        Processing,
        Processed,
        Failed,
        Published
    }
}

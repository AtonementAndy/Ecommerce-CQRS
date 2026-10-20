
namespace Ecommerce.Application.Common.Events
{
    public record ProductDeletedEvent(Guid Id, int Version);
}

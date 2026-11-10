
namespace Ecommerce.Application.Common.Events
{
    public record ProductDeletedEvent(Ulid Id, int Version);
}

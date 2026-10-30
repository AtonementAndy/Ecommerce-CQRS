using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.Common.Events
{
    public record ProductUpdatedEvent(
        [property: BsonId]
        Guid Id,
        [Required] string Name,
        [Required] string Description,
        [Required] decimal Price,
        [Required] int Stock,
        DateTime UpdatedAt,
        int Version
    );
}

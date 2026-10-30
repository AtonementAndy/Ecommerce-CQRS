using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.Common.Events
{
    public record ProductCreatedEvent(
        Guid Id,
        [Required] string Name,
        [Required] string Description,
        [Required] decimal Price,
        [Required] int Stock,
        DateTime CreatedAt,
        int Version
    );
}

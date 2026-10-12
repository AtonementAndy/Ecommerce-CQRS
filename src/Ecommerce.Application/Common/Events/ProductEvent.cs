using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.Common.Events
{
    public record ProductEvent(
        Guid Id,
        [Required] string Name,
        [Required] string Description,
        [Required] decimal Price,
        [Required] int Stock,
        DateTime CreatedAt
    );
}

namespace Ecommerce.Application.Dtos
{
    public record ProductResponse(
         Guid Id,
         string Name,
         string Description,
         decimal Price,
         int Stock,
         DateTime CreatedAt,
         DateTime UpdatedAt,
         int Version
    );
}

namespace Ecommerce.Application.Dtos
{
    public record ProductResponse(
         Ulid Id,
         string Name,
         string Description,
         decimal Price,
         int Stock,
         DateTime CreatedAt,
         DateTime UpdatedAt,
         int Version
    );
}

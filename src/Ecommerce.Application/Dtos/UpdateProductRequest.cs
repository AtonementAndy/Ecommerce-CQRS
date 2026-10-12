namespace Ecommerce.Application.Dtos
{
    public record UpdateProductRequest(
        string Name,
        string Description,
        decimal Price,
        int Stock
    );
}

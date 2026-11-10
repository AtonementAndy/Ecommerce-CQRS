namespace Ecommerce.Application.Dtos.Cart
{
    public sealed record CartItemDto(
        Ulid ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity,
        decimal Total
    );
}
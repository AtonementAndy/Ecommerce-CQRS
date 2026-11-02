namespace Ecommerce.Application.Dtos.Cart
{
    public sealed record CartItemDto(
        Guid ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity,
        decimal Total
    );
}
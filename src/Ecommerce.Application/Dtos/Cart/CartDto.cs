namespace Ecommerce.Application.Dtos.Cart
{
    public sealed record CartDto(
        Guid Id,
        Guid UserId,
        IReadOnlyList<CartItemDto> Items,
        decimal Total,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
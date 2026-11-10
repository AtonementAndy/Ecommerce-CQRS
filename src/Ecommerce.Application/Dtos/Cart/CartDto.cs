namespace Ecommerce.Application.Dtos.Cart
{
    public sealed record CartDto(
        Ulid Id,
        Ulid UserId,
        IReadOnlyList<CartItemDto> Items,
        decimal Total,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
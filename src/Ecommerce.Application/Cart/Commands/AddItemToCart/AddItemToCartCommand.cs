using MediatR;

namespace Ecommerce.Application.Cart.Commands.AddItemToCart
{
    public sealed record AddItemToCartCommand(
        Ulid UserId,
        Ulid ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity
    ) : IRequest;
}
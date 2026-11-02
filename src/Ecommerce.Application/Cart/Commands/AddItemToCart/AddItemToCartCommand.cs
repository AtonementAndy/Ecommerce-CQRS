using MediatR;

namespace Ecommerce.Application.Cart.Commands.AddItemToCart
{
    public sealed record AddItemToCartCommand(
        Guid UserId,
        Guid ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity
    ) : IRequest;
}
using MediatR;

namespace Ecommerce.Application.Cart.Commands.RemoveItemFromCart
{
    public sealed record RemoveItemFromCartCommand(
        Guid UserId,
        Guid ProductId    
    ) : IRequest;
}
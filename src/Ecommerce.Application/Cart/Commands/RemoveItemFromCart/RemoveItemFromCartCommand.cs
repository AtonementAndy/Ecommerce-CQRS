using MediatR;

namespace Ecommerce.Application.Cart.Commands.RemoveItemFromCart
{
    public sealed record RemoveItemFromCartCommand(
        Ulid UserId,
        Ulid ProductId    
    ) : IRequest;
}
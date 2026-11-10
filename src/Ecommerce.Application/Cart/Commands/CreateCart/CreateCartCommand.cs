using MediatR;

namespace Ecommerce.Application.Cart.Commands.CreateCart
{
    public sealed record CreateCartCommand(
        Ulid UserId
    ) : IRequest<Ulid>;
}
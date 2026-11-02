using MediatR;

namespace Ecommerce.Application.Cart.Commands.CreateCart
{
    public sealed record CreateCartCommand(
        Guid UserId
    ) : IRequest<Guid>;
}
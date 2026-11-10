using Ecommerce.Application.Dtos.Cart;
using MediatR;

namespace Ecommerce.Application.Cart.Queries
{
    public sealed record GetCartByUserIdQuery(
        Ulid UserId
    ) : IRequest<CartDto?>;
}
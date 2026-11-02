using Ecommerce.Application.Dtos.Cart;
using Ecommerce.Domain.Interfaces;
using MediatR;

namespace Ecommerce.Application.Cart.Queries;

public sealed class GetCartByUserIdQueryHandler(
    ICartReadRepository cartReadRepository) : IRequestHandler<GetCartByUserIdQuery, CartDto?>
{
    public async Task<CartDto?> Handle(GetCartByUserIdQuery request, CancellationToken cancellationToken)
    {
        var cart = await cartReadRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (cart is null)
            return null;

        return new CartDto(
            cart.Id,
            cart.UserId,
            cart.Items.Select(i => new CartItemDto(
                i.ProductId,
                i.ProductName,
                i.UnitPrice.Amount,
                i.Quantity.Value,
                i.UnitPrice.Amount * i.Quantity.Value
            )).ToList(),
            cart.GetTotal().Amount,
            cart.CreatedAt,
            cart.UpdatedAt
        );
    }
}
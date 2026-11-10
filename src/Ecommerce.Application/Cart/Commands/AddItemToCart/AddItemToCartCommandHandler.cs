using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.ValueObjects;
using MediatR;
using DomainCart = Ecommerce.Domain.Entities.Cart;

namespace Ecommerce.Application.Cart.Commands.AddItemToCart
{
    public sealed class AddItemToCartCommandHandler(
        ICartWriteRepository cartWriteRepository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<AddItemToCartCommand>
    {
        public async Task Handle(AddItemToCartCommand request, CancellationToken cancellationToken)
        {
            var cart = await cartWriteRepository.GetByUserIdAsync(request.UserId, cancellationToken);

            if (cart is null)
            {
                cart = new DomainCart(request.UserId);   
                await cartWriteRepository.AddAsync(cart, cancellationToken); 
            }

            var unitPrice = new Money(request.UnitPrice);
            var quantity = new Quantity(request.Quantity);
            
            cart.AddItem(request.ProductId, request.ProductName, unitPrice, quantity);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
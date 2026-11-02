using Ecommerce.Domain.Interfaces;
using MediatR;

namespace Ecommerce.Application.Cart.Commands.RemoveItemFromCart
{
    public sealed class RemoveItemFromCartCommandHandler(
        ICartWriteRepository cartWriteRepository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<RemoveItemFromCartCommand>
    {
        public async Task Handle(RemoveItemFromCartCommand request, CancellationToken cancellationToken)
        {
            var cart = await cartWriteRepository.GetByUserIdAsync(request.UserId, cancellationToken)
                ?? throw new InvalidOperationException("Cart not found for the user.");

            cart.RemoveItem(request.ProductId);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
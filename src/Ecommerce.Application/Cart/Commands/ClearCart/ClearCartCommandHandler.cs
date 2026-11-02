using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Domain.Interfaces;
using MediatR;

namespace Ecommerce.Application.Cart.Commands.ClearCart;

public sealed class ClearCartCommandHandler(
    ICartWriteRepository cartRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ClearCartCommand>
{
    public async Task Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByUserIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Cart), request.UserId);

        cart.Clear();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
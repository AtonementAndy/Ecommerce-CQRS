using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Domain.Interfaces;
using MediatR;

namespace Ecommerce.Application.Cart.Commands.CreateCart;

public sealed class CreateCartCommandHandler(
    ICartWriteRepository cartRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateCartCommand, Guid>
{
    public async Task<Guid> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        var existingCart = await cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (existingCart is not null)
            throw new ConflictException("User already has a cart.");

        var cart = new Domain.Entities.Cart(request.UserId);

        await cartRepository.AddAsync(cart, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return cart.Id;
    }
}
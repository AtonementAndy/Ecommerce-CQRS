using Ecommerce.Application.Common.OutBox;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Application.Common.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler(
        IProductRepository repository,
        IUnitOfWork unitOfWork,
        IOutboxWriter outboxWriter
        ) : IRequestHandler<UpdateProductCommand, Guid>
    {
        public async Task<Guid> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await repository.UpdateAsync(
                request.Id,
                request.Name,
                request.Description,
                request.Price,
                request.Stock,
                cancellationToken
            ) ?? throw new KeyNotFoundException($"Product with ID {request.Id} not found.");

            var eventMessage = new ProductCreatedEvent(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Stock,
                product.UpdatedAt,
                product.Version
            );

            try
            {
                await outboxWriter.EnqueueAsync("Product", eventMessage, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // O produto foi alterado por outro processo/usuário entre o FindAsync e o SaveChanges
                throw new InvalidOperationException(
                    $"The product with ID {request.Id} was modified by another user. Please refresh and try again.");
            }

            return product.Id;
        }
    }
}

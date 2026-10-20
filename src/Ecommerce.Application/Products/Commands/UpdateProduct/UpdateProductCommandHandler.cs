using Ecommerce.Application.Common.OutBox;
using Ecommerce.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Infrastructure.Messaging.Events;

namespace Ecommerce.Application.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler(
        IProductWriteRepository repository,
        IUnitOfWork unitOfWork,
        IOutboxWriter outboxWriter
        ) : IRequestHandler<UpdateProductCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await repository.UpdateAsync(
                request.Id,
                request.Name,
                request.Description,
                request.Price,
                request.Stock,
                request.ExpectedVersion,
                cancellationToken
            ) ?? throw new KeyNotFoundException($"Product with ID {request.Id} not found.");

            var eventMessage = new ProductUpdatedEvent(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Stock,
                product.UpdatedAt,
                product.Version

            );

            await outboxWriter.EnqueueAsync("Product", eventMessage, cancellationToken);

            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException(
                    $"The product with ID {request.Id} was modified by another user. Please refresh and try again.");
            }

            return Unit.Value;
        }
    }
}

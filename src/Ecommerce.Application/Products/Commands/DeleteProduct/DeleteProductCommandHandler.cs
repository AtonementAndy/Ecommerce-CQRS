using Ecommerce.Application.Common.Events;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.OutBox;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using MediatR;

namespace Ecommerce.Application.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandHandler(
        IProductWriteRepository repository, 
        IUnitOfWork unitOfWork,
        IOutboxWriter outboxWriter) : IRequestHandler<DeleteProductCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var deleted = await repository.DeleteAsync(
                request.Id, 
                request.Version,
                cancellationToken
            );

            if (!deleted)
                throw new NotFoundException(nameof(Product), request.Id);

            var eventMessage = new ProductDeletedEvent(
                request.Id, 
                request.Version + 1
            );

            await outboxWriter.EnqueueAsync("Product", eventMessage, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;

        }
    }
}

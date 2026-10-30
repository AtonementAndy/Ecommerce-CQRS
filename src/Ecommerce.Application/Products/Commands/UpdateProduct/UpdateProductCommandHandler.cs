using Ecommerce.Application.Common.OutBox;
using Ecommerce.Domain.Interfaces;
using MediatR;
using Ecommerce.Application.Common.Events;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Domain.Entities;

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
            ) ?? throw new NotFoundException(nameof(Product), request.Id);

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

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}

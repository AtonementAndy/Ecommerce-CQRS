using Ecommerce.Application.Common.Events;
using Ecommerce.Application.Common.OutBox;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Commands.CreateProduct
{
    public sealed class CreateProductCommandHandler(
        IProductRepository repository,
        IOutboxWriter outboxWriter,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, Guid>
    {
        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product(
                request.Name,
                request.Description,
                request.Price,
                request.Stock
            );

            await repository.AddAsync(product, cancellationToken);

            var eventMessage = new ProductCreatedEvent(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Stock,
                product.CreatedAt,
                product.Version
            );

            await outboxWriter.EnqueueAsync("Product", eventMessage, cancellationToken);

            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Products_Name") == true)
            {
                throw new InvalidOperationException($"There is already a registered product with the name '{request.Name}'.");
            }

            return product.Id;
        }
    }
}

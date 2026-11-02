using MediatR;

namespace Ecommerce.Application.Commands.Products.CreateProduct
{
        public sealed record CreateProductCommand(
        string Name,
        string Description,
        decimal Price,
        int Stock
    ) : IRequest<Guid>;
}

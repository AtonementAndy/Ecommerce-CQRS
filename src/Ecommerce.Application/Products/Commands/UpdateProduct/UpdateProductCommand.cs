using MediatR;

namespace Ecommerce.Application.Products.Commands.UpdateProduct
{
    public record UpdateProductCommand(
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        int Stock,
        int ExpectedVersion
        ) : IRequest<Unit>;
}
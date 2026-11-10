using MediatR;

namespace Ecommerce.Application.Commands.Products.UpdateProduct
{
    public sealed record UpdateProductCommand(
        Ulid Id,
        string Name,
        string Description,
        decimal Price,
        int Stock,
        int ExpectedVersion
        ) : IRequest<Unit>;
}
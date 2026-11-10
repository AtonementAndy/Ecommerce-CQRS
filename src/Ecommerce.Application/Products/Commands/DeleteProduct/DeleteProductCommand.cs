using MediatR;

namespace Ecommerce.Application.Commands.Products.DeleteProduct
{
    public record DeleteProductCommand(
        Ulid Id, 
        int Version
    ) : IRequest<Unit>;
}

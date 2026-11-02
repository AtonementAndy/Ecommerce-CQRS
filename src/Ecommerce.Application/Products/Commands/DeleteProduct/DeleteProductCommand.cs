using MediatR;

namespace Ecommerce.Application.Commands.Products.DeleteProduct
{
    public record DeleteProductCommand(
        Guid Id, 
        int Version
    ) : IRequest<Unit>;
}

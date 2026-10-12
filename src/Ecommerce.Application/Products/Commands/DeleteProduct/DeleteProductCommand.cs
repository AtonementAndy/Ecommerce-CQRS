using MediatR;

namespace Ecommerce.Application.Products.Commands.DeleteProduct
{
    public record DeleteProductCommand(Guid Id) : IRequest<Unit>;
}

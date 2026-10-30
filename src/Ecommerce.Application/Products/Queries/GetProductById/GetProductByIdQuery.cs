using Ecommerce.Application.Dtos;
using MediatR;

namespace Ecommerce.Application.Products.Queries.GetProductById
{
    public record GetProductByIdQuery(Guid Id) : IRequest<ProductResponse?>;
}

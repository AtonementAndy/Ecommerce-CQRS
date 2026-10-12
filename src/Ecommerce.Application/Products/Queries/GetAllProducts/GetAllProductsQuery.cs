
using Ecommerce.Application.Dtos;
using MediatR;

namespace Ecommerce.Application.Products.Queries.GetAllProducts
{
    public class GetAllProductsQuery(int pageNumber, int pageSize) : IRequest<IEnumerable<ProductDto>>
    {
        public int PageNumber { get; set; } = pageNumber;
        public int PageSize { get; set; } = pageSize;
    }
}

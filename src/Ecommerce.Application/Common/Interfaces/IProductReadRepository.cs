using Ecommerce.Application.Dtos;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Common.Interfaces
{
    public interface IProductReadRepository
    {
        Task<(IEnumerable<ProductReadModel> items, long totalCount)> GetAllAsync(
            int pageNumber, 
            int pageSize, 
            CancellationToken cancellationToken
        );
        Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}

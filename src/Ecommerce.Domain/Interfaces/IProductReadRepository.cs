using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces
{
    public interface IProductReadRepository
    {
        Task<(IEnumerable<Product> items, long totalCount)> GetAllAsync(
            int pageNumber, 
            int pageSize, 
            CancellationToken cancellationToken
        );
        Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}

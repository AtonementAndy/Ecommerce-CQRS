using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<(IEnumerable<Product> items, long totalCount)> GetAllAsync(
            int pageNumber, 
            int pageSize, 
            CancellationToken cancellationToken
        );
        Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Product> AddAsync(Product productEvent, CancellationToken cancellationToken);
        Task<bool> UpdateASync(Guid id, Product product, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);
    }
}

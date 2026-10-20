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
        Task<Product?> UpdateAsync(Guid id, string name, string description, decimal price, int stock, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(Guid id, int version, CancellationToken cancellationToken);
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);
    }
}

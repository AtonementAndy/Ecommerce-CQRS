using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces
{
    public interface IProductWriteRepository
    {
        //Task<Product?> GetByIdForUpdateAsync(Guid id, CancellationToken ct);
        Task<Product> AddAsync(Product product, CancellationToken cancellationToken);
        Task<Product?> UpdateAsync(Ulid id, string name, string description, decimal price, int stock, int expextedVersion, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(Ulid id, int version, CancellationToken cancellationToken);
    }
}

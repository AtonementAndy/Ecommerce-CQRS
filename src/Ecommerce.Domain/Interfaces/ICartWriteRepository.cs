using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces
{
    public interface ICartWriteRepository
    {
        Task<Cart?> GetByUserIdAsync(Ulid userId, CancellationToken cancellationToken = default);
        Task AddAsync(Cart cart, CancellationToken cancellationToken = default);
        Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default);
    }
}
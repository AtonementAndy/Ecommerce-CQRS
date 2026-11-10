using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces
{
    public interface ICartReadRepository
    {
        Task<Cart?> GetByUserIdAsync(Ulid userId, CancellationToken cancellationToken = default);
    }
}
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repository
{
    public class CartReadRepository(EcommerceDbContext context) : ICartReadRepository
    {
        public async Task<Cart?> GetByUserIdAsync(Ulid userId, CancellationToken cancellationToken = default)
        {
            return await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        }
    }
}
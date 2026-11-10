using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repository
{
    public class CartWriteRepository(EcommerceDbContext context) : ICartWriteRepository
    {
        public async Task AddAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            await context.Carts.AddAsync(cart, cancellationToken);
        }

        public async Task<Cart?> GetByUserIdAsync(Ulid userId, CancellationToken cancellationToken = default)
        {
            return await context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        }

        public Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            context.Carts.Update(cart);
            return Task.CompletedTask;
        }
    }
}

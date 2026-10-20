using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Infrastructure.Data
{
    public class UnitOfWork(EcommerceDbContext context) : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => context.SaveChangesAsync(cancellationToken);
    }
}

using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Data;

namespace Ecommerce.Infrastructure.Repository
{
    public class ProductWriteRepository(EcommerceDbContext context) : IProductWriteRepository
    {
        public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken)
        {
            await context.Products.AddAsync(product, cancellationToken);

            return product;
        }

        public async Task<Product?> UpdateAsync(Guid id, string name, string description, decimal price, int stock, CancellationToken cancellationToken)
        {
            var product = await context.Products.FindAsync([id], cancellationToken);
            if (product is null) return null;

            //context.Entry(product).Property(p => p.Version).OriginalValue = expectedVersion;

            product.UpdateDetails(
                name,
                description,
                price,
                stock
            );

            return product;
        }

        public async Task<bool> DeleteAsync(Guid id, int expextedVersion, CancellationToken cancellationToken)
        {
            var product = await context.Products.FindAsync([id], cancellationToken);
            if (product is null) return false;

            context.Entry(product).Property(p => p.Version).OriginalValue = expextedVersion;

            context.Products.Remove(product);

            return true;
        }
    }
}

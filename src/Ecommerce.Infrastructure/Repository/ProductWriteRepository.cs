using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Data;

namespace Ecommerce.Infrastructure.Repository
{
    internal class ProductWriteRepository(EcommerceDbContext _context) : IProductWriteRepository
    {

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FindAsync([id], cancellationToken);
            if (product is null) return null;

            return product;
        }
        public async Task<bool> UpdateASync(Guid id, Product product, CancellationToken cancellationToken)
        {
            var productFound = await _context.Products.FindAsync([id], cancellationToken);
            if (productFound is null) return false;

            productFound.UpdateDetails(
                product.Name,
                product.Description,
                product.Price,
                product.Stock
            );

            _context.Products.Update(productFound);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FindAsync([id], cancellationToken);
            if (product is null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

    }
}

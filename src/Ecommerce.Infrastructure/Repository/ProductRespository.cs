using Ecommerce.Application.Dtos;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repository
{
    internal class ProductRespository : IProductRepository
    {
        private readonly EcommerceDbContext _context;
        private readonly IMongoCollection<ProductReadModel> _mongoCollection;

        public ProductRespository(EcommerceDbContext context, IMongoDatabase mongoDatabase, IConfiguration configuration)
        {
            _context = context;
            var collectionName = configuration["MongoDbSettings:CollectionName"] ?? "ProductsReadModel";
            _mongoCollection = mongoDatabase.GetCollection<ProductReadModel>(collectionName);
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            // 1. Busca pelo Guid diretamente graças ao [BsonRepresentation(BsonType.String)] do DTO
            var filter = Builders<ProductReadModel>.Filter.Eq(p => p.Id, id);
            var doc = await _mongoCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);

            if (doc is null)
                return null;

            // 2. Reconstitui o produto preservando EXATAMENTE o Id e o CreatedAt originais do Mongo
            return new Product(
                doc.Id,
                doc.Name,
                doc.Description,
                doc.Price,
                doc.Stock,
                doc.CreatedAt
            );
        }
        public async Task<(IEnumerable<Product> items, long totalCount)> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            var filter = Builders<ProductReadModel>.Filter.Empty;

            var totalCount = await _mongoCollection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

            var docs = await _mongoCollection.Find(filter)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            var items = docs.Select(doc => new Product(
                doc.Id,
                doc.Name,
                doc.Description,
                doc.Price,
                doc.Stock,
                doc.CreatedAt
            ));

            return (items, totalCount);
        }

        public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken)
        {
            await _context.Products.AddAsync(product, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
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

        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
        {
            return await _context.Products
                .AnyAsync(p => p.Name.ToLower() == name.ToLower(), cancellationToken);
        }
    }
}
using Ecommerce.Application.Dtos;
using Ecommerce.Domain.Entities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Ecommerce.Domain.Interfaces;
namespace Ecommerce.Infrastructure.Repository
{
    public class ProductReadRepository : IProductReadRepository
    {
        private readonly IMongoCollection<ProductReadModel> _mongoCollection;

        public ProductReadRepository(IMongoDatabase mongoDatabase, IConfiguration configuration)
        {
            var collectionName = configuration["MongoDbSettings:CollectionName"] ?? "ProductsReadModel";
            _mongoCollection = mongoDatabase.GetCollection<ProductReadModel>(collectionName);
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var filter = Builders<ProductReadModel>.Filter.And(
                Builders<ProductReadModel>.Filter.Eq(p => p.Id, id),
                Builders<ProductReadModel>.Filter.Eq(p => p.IsDeleted, false)
            );
                            
            var doc = await _mongoCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);

            if (doc is null)
                return null;

            return new Product(
                doc.Id,
                doc.Name,
                doc.Description,
                doc.Price,
                doc.Stock,
                doc.CreatedAt,
                doc.UpdatedAt,
                doc.Version
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
                doc.CreatedAt,
                doc.UpdatedAt,
                doc.Version
            ));

            return (items, totalCount);
        }
    }
}
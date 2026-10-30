using Ecommerce.Application.Dtos;
using Ecommerce.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Ecommerce.Infrastructure.Repository
{
    internal class ProductReadRepository : IProductReadRepository
    {
        private readonly IMongoCollection<ProductReadModel> _mongoCollection;

        public ProductReadRepository(IMongoDatabase mongoDatabase, IConfiguration configuration)
        {
            var collectionName = configuration["MongoDbSettings:CollectionName"] ?? "ProductsReadModel";
            _mongoCollection = mongoDatabase.GetCollection<ProductReadModel>(collectionName);
        }

        public async Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            //var filter = Builders<ProductReadModel>.Filter.And(
            //    Builders<ProductReadModel>.Filter.Eq(p => p.Id, id),
            //    Builders<ProductReadModel>.Filter.Eq(p => p.IsDeleted, false)
            //);
            var filter = Builders<ProductReadModel>.Filter.Eq(p => p.Id, id);
                            
            var doc = await _mongoCollection
                .Find(filter)
                .FirstOrDefaultAsync(cancellationToken);

            if (doc is null)
                return null;

            return doc;
        }

        public async Task<(IEnumerable<ProductReadModel> items, long totalCount)> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            var filter = Builders<ProductReadModel>.Filter.Empty;
            var totalCount = await _mongoCollection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

            var items = await _mongoCollection.Find(filter)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
using Ecommerce.Application.Common.Cache;
using Ecommerce.Application.Dtos;
using Ecommerce.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using System.Text.Json;

namespace Ecommerce.Application.Products.Queries.GetProductById
{
    internal class GetProductByIdQueryHandler(IProductRepository repository, IDistributedCache cache) : IRequestHandler<GetProductByIdQuery, ProductDto?>
    {
        public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"product:{request.Id}";

            // 1. Tenta buscar primeiro no REDIS (Cache em Memória)
            var cachedProduct = await cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedProduct))
            {
                return JsonSerializer.Deserialize<ProductDto>(cachedProduct);
            }

            // 2. Se NÃO está no Redis (Cache Miss), busca no MONGODB (Sem passar pelo SQL Server!)
            var product = await repository.GetByIdAsync(request.Id, cancellationToken);

            if (product != null)
            {
                await cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(product),
                    CacheOptions.DefaultExpiration,
                    cancellationToken
                );
            }

            if (product is null)
                return null;

            var productDto = new ProductDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Stock,
                product.CreatedAt
            );

            return productDto;
        }
    }
}

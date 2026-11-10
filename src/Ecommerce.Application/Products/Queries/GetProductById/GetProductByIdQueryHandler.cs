using Ecommerce.Application.Common.Cache;
using Ecommerce.Application.Dtos;
using Ecommerce.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Ecommerce.Application.Products.Queries.GetProductById
{
    internal class GetProductByIdQueryHandler(IProductReadRepository repository, IDistributedCache cache) : IRequestHandler<GetProductByIdQuery, ProductResponse?>
    {
        public async Task<ProductResponse?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"product:{request.Id}";

            // 1. Tenta buscar primeiro no REDIS (Cache em Memória)
            var cachedProduct = await cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedProduct))
            {
                return JsonSerializer.Deserialize<ProductResponse>(cachedProduct);
            }

            var product = await repository.GetByIdAsync(request.Id, cancellationToken);

            if (product is null) return null;

            var productDto = new ProductResponse(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Stock,
                product.CreatedAt,
                product.UpdatedAt,
                product.Version
            );

            await cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(productDto),
                CacheOptions.DefaultExpiration,
                cancellationToken
            );

            return productDto;
        }
    }
}

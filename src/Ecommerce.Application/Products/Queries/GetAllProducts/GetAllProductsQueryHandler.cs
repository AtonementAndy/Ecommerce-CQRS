using Ecommerce.Application.Common.Cache;
using Ecommerce.Application.Dtos;
using Ecommerce.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Ecommerce.Application.Products.Queries.GetAllProducts
{
    public class GetAllProductsQueryHandler(IDistributedCache cache, IProductRepository repository) : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
    {
        public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

            var cacheKey = CacheKeys.GetProductsKey(pageNumber, pageSize);

            var cachedProducts = await cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedProducts))
            {
                var allCacheProducts = JsonSerializer.Deserialize<IEnumerable<ProductDto>>(cachedProducts);
                if (allCacheProducts != null) 
                    return allCacheProducts;
            }

            var products = await repository.GetAllAsync(pageNumber, pageSize, cancellationToken);
            var productsDto = products.items.Select(p => 
                new ProductDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.Stock,
                    p.CreatedAt
                )
                
            ).ToList();

            if (productsDto.Count != 0)
            {
                await cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(productsDto),
                    CacheOptions.DefaultExpiration,
                    cancellationToken
                );
            }

            return productsDto;
        }
    }
}

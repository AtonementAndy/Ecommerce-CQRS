using Ecommerce.Application.Common.Cache;
using Ecommerce.Application.Dtos;
using Ecommerce.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Ecommerce.Application.Products.Queries.GetAllProducts
{
    public class GetAllProductsQueryHandler(
        IDistributedCache cache, 
        IProductReadRepository repository) : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
    {
        public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

            var listVersion = await CacheInvalidator.GetProductsListVersionAsync(cache, cancellationToken);

            var cacheKey = CacheKeys.GetProductsKey(listVersion, pageNumber, pageSize);

            var cachedProducts = await cache.GetStringAsync(cacheKey, cancellationToken);
            
            if (!string.IsNullOrEmpty(cachedProducts))
            {
                var allCacheProducts = JsonSerializer.Deserialize<IEnumerable<ProductDto>>(cachedProducts);
                if (allCacheProducts != null) 
                    return allCacheProducts;
            }

            var (items, totalCount) = await repository.GetAllAsync(pageNumber, pageSize, cancellationToken);
            var productsDto = items.Select(p => 
                new ProductDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.Stock,
                    p.CreatedAt,
                    p.UpdatedAt,
                    p.Version
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

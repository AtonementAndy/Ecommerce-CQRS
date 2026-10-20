using Microsoft.Extensions.Caching.Distributed;

namespace Ecommerce.Application.Common.Cache
{
    public static class CacheInvalidator
    {
        public static async Task InvalidateProductCachesAsync(
            IDistributedCache cache,
            Guid productId,
            CancellationToken cancellationToken
        )
        {
            // Invalidate the specific product
            await cache.RemoveAsync(CacheKeys.GetProductByIdKey(productId), cancellationToken);

            // Invalidate All lists, incrementing the version
            await BumpProductsListVersionAsync(cache, cancellationToken);
        }

        private static async Task BumpProductsListVersionAsync(
            IDistributedCache cache,
            CancellationToken cancellationToken
        )
        {
            var current = await cache.GetStringAsync(CacheKeys.ProductsListVersion(), cancellationToken);
            var next = (int.TryParse(current, out var v) ? v : 0) + 1;

            await cache.SetStringAsync(
                CacheKeys.ProductsListVersion(),
                next.ToString(),
                CacheOptions.DefaultExpiration,
                cancellationToken
            );
        }

        public static async Task<int> GetProductsListVersionAsync(
            IDistributedCache cache,
            CancellationToken cancellationToken
        )
        {
            var current = await cache.GetStringAsync(CacheKeys.ProductsListVersion(), cancellationToken);
            return int.TryParse(current, out var v) ? v : 0;
        }
    }
}

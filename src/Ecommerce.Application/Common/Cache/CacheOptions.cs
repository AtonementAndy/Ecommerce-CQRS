using Microsoft.Extensions.Caching.Distributed;

namespace Ecommerce.Application.Common.Cache
{
    public static class CacheOptions
    {
        public static DistributedCacheEntryOptions DefaultExpiration => new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
    }
}

namespace Ecommerce.Application.Common.Cache
{
    public static class CacheKeys
    {
        public static string ProductsListVersion() => "products:list:version";
        public static string GetProductsKey(int version, int pageNumber, int pageSize) 
            => $"products:list:v{version}:page:{pageNumber}:size:{pageSize}";
        public static string GetProductByIdKey(Guid id) => $"product:{id}";
    }
}
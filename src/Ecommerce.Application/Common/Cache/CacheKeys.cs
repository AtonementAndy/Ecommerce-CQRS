namespace Ecommerce.Application.Common.Cache
{
    public static class CacheKeys
    {
        // Chave para listar todos ou paginados (você pode concatenar os parâmetros)
        public static string GetProductsKey(int pageNumber, int pageSize) => $"products:page:{pageNumber}:size:{pageSize}";

        // Chave para um produto específico baseado no ID
        public static string GetProductByIdKey(Guid id) => $"product:{id}";

        // Chave genérica caso queira para outras entidades no futuro
        public static string GetEntityKey(string entityName, Guid id) => $"{entityName.ToLower()}:{id}";
    }
}
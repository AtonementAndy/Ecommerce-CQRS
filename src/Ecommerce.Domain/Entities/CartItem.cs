using Ecommerce.Domain.ValueObjects;

namespace Ecommerce.Domain.Entities
{
    public class CartItem
    {
        public Guid ProductId { get; private set; }
        public string ProductName { get; private set; } = string.Empty;// Snapshot do nome
        public Money UnitPrice { get; private set; }    // Snapshot do preço
        public Quantity Quantity { get; private set; }

        // Construtor privado para o EF Core
        private CartItem() { }

        internal CartItem(Guid productId, string productName, Money unitPrice, Quantity quantity)
        {
            ProductId = productId;
            ProductName = productName;
            UnitPrice = unitPrice;
            Quantity = quantity;
        }

        internal void AddQuantity(Quantity quantityToAdd)
        {
            Quantity += quantityToAdd;
        }
    }
}
using Ecommerce.Domain.ValueObjects;

namespace Ecommerce.Domain.Entities
{
    public class CartItem
    {
        public Guid ProductId { get; private set; }
        public string ProductName { get; private set; } = string.Empty;
        public Money UnitPrice { get; private set; }  
        public Quantity Quantity { get; private set; }

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
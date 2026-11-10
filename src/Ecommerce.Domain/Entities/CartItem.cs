using Ecommerce.Domain.ValueObjects;

namespace Ecommerce.Domain.Entities
{
    public class CartItem
    {
        public Ulid ProductId { get; private set; }
        public string ProductName { get; private set; } = string.Empty;
        public Money UnitPrice { get; private set; }  
        public Quantity Quantity { get; private set; }

        private CartItem() { }

        internal CartItem(Ulid productId, string productName, Money unitPrice, Quantity quantity)
        {
            ProductId = Ulid.NewUlid();
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
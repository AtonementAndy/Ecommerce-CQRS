using Ecommerce.Domain.ValueObjects;

namespace Ecommerce.Domain.Entities
{
    public class Cart
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        private readonly List<CartItem> _items = [];
        public IReadOnlyCollection<CartItem> Item => _items.AsReadOnly();

        public Cart(Guid userId)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddItem(Guid productId, string productName, Money unitPrice, Quantity quantity)
        {
            var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
                existingItem.AddQuantity(quantity);
            else
                _items.Add(new CartItem(productId, productName, unitPrice, quantity));

            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveItem(Guid productId)
        {
            var item = _items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                _items.Remove(item);
                UpdatedAt = DateTime.UtcNow;
            }
        }
        public Money GetTotal()
        {
            // Utilizando o Value Object para somar tudo de forma segura
            var total = _items.Aggregate(new Money(0m), (acc, item) => acc + (item.UnitPrice * item.Quantity.Value));
            return total;
        }
    }
}

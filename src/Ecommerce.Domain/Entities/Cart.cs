using Ecommerce.Domain.Exceptions;
using Ecommerce.Domain.ValueObjects;

namespace Ecommerce.Domain.Entities
{
    public class Cart
    {
        public Ulid Id { get; private set; }
        public Ulid UserId { get;  private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        private readonly List<CartItem> _items = [];
        public IReadOnlyCollection<CartItem> Items => _items;

        private Cart() { }

        public Cart(Ulid userId)
        {
            if (userId == Ulid.Empty)
                throw new DomainException("UserId is required.");

            Id = Ulid.NewUlid();
            UserId = userId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddItem(Ulid productId, string productName, Money unitPrice, Quantity quantity)
        {
            if (productId == Ulid.Empty)
                throw new DomainException("ProductId is required.");

            if (string.IsNullOrWhiteSpace(productName))
                throw new DomainException("Product name is required.");

            var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem is not null)
                existingItem.AddQuantity(quantity);
            else
                _items.Add(new CartItem(productId, productName, unitPrice, quantity));

            Touch();
        }

        public void RemoveItem(Ulid productId)
        {
            var item = _items.FirstOrDefault(i => i.ProductId == productId)
                ?? throw new DomainException("Item not found in cart.");
            
            _items.Remove(item);
            Touch();
        }

        public void Clear()
        {
            _items.Clear();
            Touch();
        }

        public Money GetTotal()
        {
            if(_items.Count == 0)
                return new Money(0m);
            
            return _items.Aggregate(
                new Money(0m),
                (acc, item) => acc + (item.UnitPrice * item.Quantity.Value)
            );
        }

        public void Touch() => UpdatedAt = DateTime.UtcNow;
    }
}

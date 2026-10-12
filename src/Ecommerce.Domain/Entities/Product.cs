namespace Ecommerce.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public decimal Price { get; private set; }
        public int Stock { get; private set; }
        public DateTime CreatedAt { get; private set; }

        protected Product() { }

        public Product(string name, string description, decimal price, int stock)
        {
            //Id = id == Guid.Empty ? Guid.NewGuid() : id;
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            Price = price;
            Stock = stock;
            CreatedAt = DateTime.UtcNow;
        }

        public Product(Guid id, string name, string description, decimal price, int stock, DateTime createdAt)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            Stock = stock;
            CreatedAt = createdAt;
        }

        public void UpdateDetails(string name, string description, decimal price, int stock)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The product name is mandatory.");

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("The description is mandatory.");

            if (price <= 0)
                throw new ArgumentException("The price is mandatory and can't be less than 0.");

            if (stock < 0)
                throw new ArgumentException("The stock can't less than 0.");

            Name = name;
            Description = description;
            Price = price;
            Stock = stock;
        }
    }
}

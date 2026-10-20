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
        public DateTime UpdatedAt { get; private set; }
        public int Version { get; private set; }

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
            Version = 1;
        }

        public Product(Guid id, string name, string description, decimal price, int stock, DateTime createdAt, DateTime updatedAt, int version)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            Stock = stock;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            Version = version;
        }

        public void UpdateDetails(string name, string description, decimal price, int stock)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The product name is required.");

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("The description is required.");

            if (price <= 0)
                throw new ArgumentException("The price is required and can't be less than 0.");

            if (stock < 0)
                throw new ArgumentException("The stock can't less than 0.");

            Name = name;
            Description = description;
            Price = price;
            Stock = stock;
            UpdatedAt = DateTime.UtcNow;
            Version++;
        }
    }
}

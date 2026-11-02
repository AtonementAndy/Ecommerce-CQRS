using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Domain.ValueObjects
{
    public readonly struct Quantity
    {
        public int Value { get; }
        public Quantity(int amount)
        {
            if (amount < 0)
                throw new DomainException("Quantity must be less than zero.");

            Value = amount;
        }

        public static Quantity operator +(Quantity a, Quantity b)
            => new(a.Value + b.Value);
    }
}

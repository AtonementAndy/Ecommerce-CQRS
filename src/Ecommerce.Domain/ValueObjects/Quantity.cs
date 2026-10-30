using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Domain.ValueObjects
{
    public readonly struct Quantity
    {
        public int Value { get; }
        public Quantity(int value)
        {
            if (value <= 0)
                throw new DomainException("Quantity must be greater than zero.");

            Value = value;
        }

        public static Quantity operator +(Quantity a, Quantity b)
            => new(a.Value + b.Value);
    }
}

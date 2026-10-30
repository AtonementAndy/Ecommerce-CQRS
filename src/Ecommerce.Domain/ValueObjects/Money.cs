using Ecommerce.Domain.Exceptions;
using System.Numerics;

namespace Ecommerce.Domain.ValueObjects
{
    public readonly struct Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency = "BRL")
        {
            if (amount <= 0) 
                throw new DomainException("Price cannot be negative.");

            if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
                throw new DomainException("Invalid currency code.");

            Amount = amount;
            Currency = currency;
        }

        public static Money operator *(Money money, int quantity)
            => new(money.Amount * quantity, money.Currency);

        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new DomainException("Cannot add different currencies.");

            return new(a.Amount + b.Amount, a.Currency);
        }
    }
}

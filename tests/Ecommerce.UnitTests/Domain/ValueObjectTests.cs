using Ecommerce.Domain.Exceptions;
using Ecommerce.Domain.ValueObjects;
using FluentAssertions;

namespace Ecommerce.UnitTests.Domain;

public class ValueObjectTests
{
    [Fact]
    public void Money_ShouldRejectNonPositiveAmount()
    {
        var act = () => new Money(0m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Money_ShouldRejectInvalidCurrency()
    {
        var act = () => new Money(1m, "US");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MoneyAddition_ShouldRejectDifferentCurrencies()
    {
        var act = () => new Money(1m, "USD") + new Money(1m, "BRL");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MoneyMultiplication_ShouldPreserveCurrencyAndScaleAmount()
    {
        var result = new Money(10m, "BRL") * 3;

        result.Amount.Should().Be(30m);
        result.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Quantity_ShouldRejectNonPositiveValue()
    {
        var act = () => new Quantity(0);

        act.Should().Throw<DomainException>();
    }
}

using Ecommerce.Application.Products.Commands.CreateProduct;
using Ecommerce.Application.Products.Commands.UpdateProduct;
using FluentValidation.TestHelper;

namespace Ecommerce.UnitTests.Products;

public class ProductValidationTests
{
    [Fact]
    public void CreateValidator_ShouldAcceptValidProduct()
    {
        var result = new CreateProductValidation().TestValidate(
            new CreateProductCommand("Notebook", "Portable computer", 10m, 0));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateValidator_ShouldRejectMissingFieldsAndInvalidValues()
    {
        var result = new CreateProductValidation().TestValidate(
            new CreateProductCommand("", "", 0m, -1));

        result.ShouldHaveValidationErrorFor(command => command.Name);
        result.ShouldHaveValidationErrorFor(command => command.Description);
        result.ShouldHaveValidationErrorFor(command => command.Price);
        result.ShouldHaveValidationErrorFor(command => command.Stock);
    }

    [Fact]
    public void CreateValidator_ShouldRejectFieldsExceedingMaximumLength()
    {
        var result = new CreateProductValidation().TestValidate(
            new CreateProductCommand(new string('n', 101), new string('d', 201), 10m, 0));

        result.ShouldHaveValidationErrorFor(command => command.Name);
        result.ShouldHaveValidationErrorFor(command => command.Description);
    }

    [Fact]
    public void UpdateValidator_ShouldRejectInvalidPriceAndNegativeStock()
    {
        var result = new UpdateProductValidation().TestValidate(
            new UpdateProductCommand(Guid.NewGuid(), "Name", "Description", -1m, -1, 1));

        result.ShouldHaveValidationErrorFor(command => command.Price);
        result.ShouldHaveValidationErrorFor(command => command.Stock);
    }
}

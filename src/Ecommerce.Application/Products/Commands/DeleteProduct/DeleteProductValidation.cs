using FluentValidation;

namespace Ecommerce.Application.Products.Commands.DeleteProduct
{
    public class DeleteProductValidation : AbstractValidator<DeleteProductCommand>
    {
        public DeleteProductValidation()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Product Id is required.");
                //.Must(id => id != Guid.Empty).WithMessage("Product ID must be a valid non-empty GUID.");
        }
    }
}

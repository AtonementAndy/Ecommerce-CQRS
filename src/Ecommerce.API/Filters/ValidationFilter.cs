using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ecommerce.API.Filters
{
    public class ValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument is null) continue;

                var argumentType = argument.GetType();
                var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);

                if (serviceProvider.GetService(validatorType) is IValidator validator)
                {
                    var validationContext = new ValidationContext<object>(argument);
                    var result = await validator.ValidateAsync(validationContext);

                    if (!result.IsValid)
                        throw new ValidationException(result.Errors);
                }
            }

            await next();
        }
    }
}

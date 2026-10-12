using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ecommerce.API.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if(context.Exception is KeyNotFoundException ex)
            {
                context.Result = new NotFoundObjectResult(new { error = ex.Message });
                context.ExceptionHandled = true;
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc.Filters;

namespace Ecommerce.API.Filters
{
    public class LogActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var actionName = context.ActionDescriptor.DisplayName;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[FILTER] Leaving: {actionName}");
            Console.ResetColor();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var actionName = context.ActionDescriptor.DisplayName;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[FILTER] Starting in: {actionName}");
            Console.ResetColor();
        }
    }
}

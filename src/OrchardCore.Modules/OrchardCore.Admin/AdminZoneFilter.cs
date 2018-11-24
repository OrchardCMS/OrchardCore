using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrchardCore.Admin
{
    /// <summary>
    /// This filter makes an controller that starts with Admin behave as
    /// it had the <see cref="AdminAttribute"/>.
    /// </summary>
    public class AdminZoneFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller as Controller;
            if (controller != null)
            {
                if(controller.GetType().Name.StartsWith("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    AdminAttribute.Apply(context.HttpContext);
                }
            }
        }
    }
}

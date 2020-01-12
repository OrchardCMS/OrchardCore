using System;
using System.IO;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrchardCore.Admin
{
    /// <summary>
    /// This filter makes a controller that starts with Admin behave as
    /// it had the <see cref="AdminAttribute"/>.
    /// </summary>
    public class AdminZoneFilter : IActionFilter, IPageFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionDescriptor is ControllerActionDescriptor descriptor &&
                descriptor.ControllerName == "Admin")
            {
                AdminAttribute.Apply(context.HttpContext);
            }
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            if (Path.GetFileName(context.ActionDescriptor.DisplayName).StartsWith("Admin"))
            {
                AdminAttribute.Apply(context.HttpContext);
            }
        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
        }
    }
}

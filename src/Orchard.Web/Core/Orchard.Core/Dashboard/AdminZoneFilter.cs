using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Orchard.DependencyInjection;
using Orchard.DisplayManagement.Admin;

namespace Orchard.Core.Dashboard
{
    [ScopedComponent(typeof(IFilterMetadata))]
    public class AdminZoneFilter : IActionFilter, IFilterMetadata
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller as Controller;
            if (controller != null)
            {
                if(controller.GetType().Name.StartsWith("Admin"))
                {
                    AdminAttribute.Apply(context.HttpContext);
                }
            }
        }
    }
}

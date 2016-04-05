using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.Hosting.Web.Mvc.ModelBinding
{
    public class ModelBinderAccessorFilter : IActionFilter, IFilterMetadata
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller as Controller;
            if (controller != null)
            {
                var modelBinderAccessor = context.HttpContext.RequestServices.GetRequiredService<IModelUpdaterAccessor>();
                modelBinderAccessor.ModelUpdater = new ControllerModelUpdater(controller);
            }
        }
    }
}

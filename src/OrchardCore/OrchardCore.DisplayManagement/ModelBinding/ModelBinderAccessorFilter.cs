using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.DisplayManagement.ModelBinding
{
    public class ModelBinderAccessorFilter : IActionFilter, IPageFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller as Controller;
            if (controller != null)
            {
                var modelBinderAccessor = context.HttpContext.RequestServices.GetRequiredService<IUpdateModelAccessor>();
                modelBinderAccessor.ModelUpdater = new ControllerModelUpdater(controller);
            }
        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            var page = context.HandlerInstance as PageModel;

            if (page != null)
            {
                var modelBinderAccessor = context.HttpContext.RequestServices.GetRequiredService<IUpdateModelAccessor>();
                modelBinderAccessor.ModelUpdater = new PageModelUpdater(page);
            }
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
        }
    }
}

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore.Forms.Helpers;

namespace OrchardCore.Forms.Filters
{
    // For Razor Pages IActionFilter\ActionFilterAttribute don't apply, IPageFilter have to be used instead.
    public class ImportModelStatePageFilter : IPageFilter
    {
        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            var pageModel = context.HandlerInstance as PageModel;

            if (pageModel?.TempData[ModelStateTransferAttribute.Key] is string serializedModelState)
            {
                // Only Import if we are viewing.
                if (context.Result is PageResult)
                {
                    var modelState = ModelStateHelpers.DeserializeModelState(serializedModelState);
                    context.ModelState.Merge(modelState);
                }
                else
                {
                    // Otherwise remove it.
                    pageModel.TempData.Remove(ModelStateTransferAttribute.Key);
                }
            }
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
        }
    }
}

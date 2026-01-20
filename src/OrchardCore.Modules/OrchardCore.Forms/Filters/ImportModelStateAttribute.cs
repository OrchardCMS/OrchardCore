using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore.Forms.Helpers;

namespace OrchardCore.Forms.Filters
{
    public class ImportModelStateAttribute : ModelStateTransferAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var controller = context.Controller as Controller;
            var serializedModelState = controller?.TempData[Key] as string;

            if (serializedModelState != null)
            {
                // Only Import if we are viewing.
                if (context.Result is ViewResult || context.Result is PageResult)
                {
                    var modelState = ModelStateHelpers.DeserializeModelState(serializedModelState);
                    context.ModelState.Merge(modelState);
                }
                else
                {
                    // Otherwise remove it.
                    controller.TempData.Remove(Key);
                }
            }

            base.OnActionExecuted(context);
        }
    }
}

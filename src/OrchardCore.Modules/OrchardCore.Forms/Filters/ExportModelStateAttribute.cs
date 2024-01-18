using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.Forms.Helpers;

namespace OrchardCore.Forms.Filters
{
    public class ExportModelStateAttribute : ModelStateTransferAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // Only export if ModelState is not valid.
            if (context.ModelState != null && !context.ModelState.IsValid && IsRedirect(context))
            {
                var controller = context.Controller as Controller;
                if (controller != null)
                {
                    controller.TempData[Key] = ModelStateHelpers.SerializeModelState(context.ModelState);
                }
            }

            base.OnActionExecuted(context);
        }

        private static bool IsRedirect(ActionExecutedContext context)
        {
            var result = context.Result;
            var statusCode = context.HttpContext.Response.StatusCode;

            return
                result is RedirectResult ||
                result is RedirectToRouteResult ||
                result is RedirectToActionResult ||
                result is LocalRedirectResult ||
                statusCode == (int)HttpStatusCode.Redirect ||
                statusCode == (int)HttpStatusCode.TemporaryRedirect;
        }
    }
}

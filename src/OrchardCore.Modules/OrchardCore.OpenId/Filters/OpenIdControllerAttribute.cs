using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.OpenId.ViewModels;

namespace OrchardCore.OpenId.Filters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class OpenIdControllerAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var response = context.HttpContext.GetOpenIdConnectResponse();
            if (response != null)
            {
                var provider = context.HttpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();

                context.Result = new ViewResult
                {
                    ViewName = "Error",
                    ViewData = new ViewDataDictionary(provider, context.ModelState)
                    {
                        Model = new ErrorViewModel
                        {
                            Error = response.Error,
                            ErrorDescription = response.ErrorDescription
                        }
                    }
                };

                return;
            }

            // If the request is missing, this likely means that
            // this endpoint was not enabled in the settings.
            // In this case, simply return a 404 response.
            var request = context.HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                context.Result = new NotFoundResult();
            }
        }
    }
}

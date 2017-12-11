using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Apis.JsonApi
{
    public class JsonApiFilter : IAsyncActionFilter
    {
        private readonly IJsonApiResultManager _manager;

        private static string ContentType = "application/vnd.api+json";

        public JsonApiFilter(
            IJsonApiResultManager manager)
        {
            _manager = manager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var isJsonApiRequest = context.HttpContext.Request.ContentType == ContentType;

            var actionExecutedContext = await next();

            if (!isJsonApiRequest)
            {
                return;
            }

            var result = actionExecutedContext.Result as ObjectResult;

            if (result == null)
            {
                return;
            }

            actionExecutedContext.HttpContext.Response.ContentType = ContentType;

            var urlHelper = actionExecutedContext
                .HttpContext
                .RequestServices
                .GetRequiredService<IUrlHelperFactory>()
                .GetUrlHelper(actionExecutedContext);

            actionExecutedContext.Result = await Build(urlHelper, result.Value);
        }

        private async Task<JsonResult> Build(IUrlHelper urlHelper, object actionValue)
        {
            var result = await _manager.Build(urlHelper, actionValue);

            return new JsonResult(result);
        }
    }
}

//using System;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using Microsoft.AspNetCore.Mvc.Routing;
//using Microsoft.Extensions.DependencyInjection;

//namespace OrchardCore.Apis.JsonApi
//{
//    public class JsonApiFilter : IAsyncActionFilter
//    {
//        private readonly IJsonApiResultManager _manager;
//        private readonly JsonApiSettings _settings;

//        private static string ContentType = "application/vnd.api+json";

//        public JsonApiFilter(
//            IJsonApiResultManager manager)
//        {
//            _manager = manager;
//            _settings = new JsonApiSettings();
//        }

//        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
//        {
//            var actionExecutedContext = await next();

//            if (!IsJsonApiRequest(context.HttpContext))
//            {
//                return;
//            }

//            // Only deal with Object Results...
//            var result = actionExecutedContext.Result as ObjectResult;

//            if (result == null)
//            {
//                return;
//            }

//            actionExecutedContext.HttpContext.Response.ContentType = ContentType;

//            var urlHelper = actionExecutedContext
//                .HttpContext
//                .RequestServices
//                .GetRequiredService<IUrlHelperFactory>()
//                .GetUrlHelper(actionExecutedContext);

//            actionExecutedContext.Result = await Build(urlHelper, result.Value);
//        }

//        private bool IsJsonApiRequest(HttpContext context)
//        {
//            return context.Request.Path.StartsWithSegments(_settings.Path)
//                && context.Request.ContentType.StartsWith(ContentType, StringComparison.OrdinalIgnoreCase);
//        }

//        private async Task<JsonResult> Build(IUrlHelper urlHelper, object actionValue)
//        {
//            var result = await _manager.Build(urlHelper, actionValue);

//            return new JsonResult(result);
//        }
//    }
//}

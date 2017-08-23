using JsonApiSerializer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Api;
using Orchard.Environment.Shell;

namespace Orchard.JsonApi.Filters
{
    public class JsonApiFilter : IActionFilter, IResultFilter
    {
        private readonly ShellSettings _shellSettings;
        private readonly IApiContentManager _contentManager;

        private string _tenantPath;

        private static string ContentType = "application/vnd.api+json";

        public JsonApiFilter(
            ShellSettings shellSettings,
            IApiContentManager contentManager)
        {
            _shellSettings = shellSettings;
            _contentManager = contentManager;

            _tenantPath = "/" + _shellSettings.RequestUrlPrefix;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var httpContext = filterContext.HttpContext;

            if (httpContext.Request.ContentType != ContentType)
            {
                return;
            }

            var result = (filterContext.Result as ObjectResult);

            if (result == null)
            {
                return;
            }

            var contentItem = result.Value as ContentItem;

            if (contentItem == null)
            {
                return;
            }

            httpContext.Response.ContentType = ContentType;

            var item = _contentManager
                .BuildAsync(contentItem, new UrlHelper(filterContext), null)
                .GetAwaiter()
                .GetResult();

            filterContext.Result = new JsonResult(item, new JsonApiSerializerSettings());
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            
        }
    }
}

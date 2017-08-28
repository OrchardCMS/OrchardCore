using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiSerializer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.Environment.Shell;

namespace Orchard.JsonApi.Filters
{
    public class JsonApiFilter : IAsyncActionFilter
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


            var urlHelper = actionExecutedContext
                .HttpContext
                .RequestServices
                .GetRequiredService<IUrlHelperFactory>()
                .GetUrlHelper(actionExecutedContext);

            actionExecutedContext.HttpContext.Response.ContentType = ContentType;

            var contentItem = result.Value as ContentItem;

            if (contentItem != null)
            {
                await BuildSingularContentItem(actionExecutedContext, contentItem);
                return;
            }

            var contentItems = result.Value as IEnumerable<ContentItem>;

            if (contentItems != null)
            {
                await BuildMultipleContentItems(actionExecutedContext, contentItems);
                return;
            }
        }

        private async Task BuildSingularContentItem(ActionExecutedContext actionExecutedContext, ContentItem contentItem)
        {
            var urlHelper = actionExecutedContext
                .HttpContext
                .RequestServices
                .GetRequiredService<IUrlHelperFactory>()
                .GetUrlHelper(actionExecutedContext);

            var item = await _contentManager
                .BuildAsync(contentItem, urlHelper, null);

            actionExecutedContext.Result = new JsonResult(item, new JsonApiSerializerSettings());
        }

        private Task BuildMultipleContentItems(ActionExecutedContext actionExecutedContext, IEnumerable<ContentItem> contentItems)
        {
            var urlHelper = actionExecutedContext
    .HttpContext
    .RequestServices
    .GetRequiredService<IUrlHelperFactory>()
    .GetUrlHelper(actionExecutedContext);

            var items = contentItems.Select(contentItem => _contentManager
                .BuildAsync(contentItem, urlHelper, null).Result)
                .ToArray();

            actionExecutedContext.Result = new JsonResult(items, new JsonApiSerializerSettings());

            return Task.CompletedTask;
        }
    }
}



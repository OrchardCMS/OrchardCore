using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiSerializer;
using JsonApiSerializer.JsonApi;
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

            actionExecutedContext.HttpContext.Response.ContentType = ContentType;

            var urlHelper = actionExecutedContext
                .HttpContext
                .RequestServices
                .GetRequiredService<IUrlHelperFactory>()
                .GetUrlHelper(actionExecutedContext);

            actionExecutedContext.Result = await Build(urlHelper, result.Value);
        }

        private Task<JsonResult> Build(IUrlHelper urlHelper, object actionValue)
        {
            var contentItem = actionValue as ContentItem;

            if (contentItem != null)
            {
                return BuildContentItem(urlHelper, contentItem);
            }

            var contentItems = actionValue as IEnumerable<ContentItem>;

            if (contentItem != null)
            {
                return BuildContentItems(urlHelper, contentItems.ToArray());
            }

            contentItems = actionValue as ContentItem[];

            if (contentItems != null)
            {
                return BuildContentItems(urlHelper, contentItems.ToArray());
            }

            return null;
        }

        private async Task<JsonResult> BuildContentItem(IUrlHelper urlHelper, ContentItem contentItem)
        {
            DocumentRoot<ApiItem> document = new DocumentRoot<ApiItem>();
            document.Data = await _contentManager.BuildAsync(contentItem, urlHelper, null);

            return new JsonResult(document, new JsonApiSerializerSettings());
        }

        private async Task<JsonResult> BuildContentItems(IUrlHelper urlHelper, ContentItem[] contentItems)
        {
            var items = new List<ApiItem>();

            foreach (var contentItem in contentItems)
            {
                items.Add(await _contentManager.BuildAsync(contentItem, urlHelper, null));
            }

            var document = new DocumentRoot<List<ApiItem>>();
            document.Data = items;

            return new JsonResult(document, new JsonApiSerializerSettings());
        }
    }
}

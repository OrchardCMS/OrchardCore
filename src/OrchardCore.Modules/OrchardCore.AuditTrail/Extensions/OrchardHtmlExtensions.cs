using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using System.Threading.Tasks;

namespace OrchardCore.AuditTrail.Extensions
{
    public static class OrchardHtmlExtensions
    {
        public static async Task<string> GetItemEditLinkAsync(
            this IOrchardHelper orchardHelper,
            string linkText,
            ContentItem contentItem)
        {
            var urlHelperFactory = orchardHelper.HttpContext.RequestServices.GetService<IUrlHelperFactory>();
            var viewContextAccessor = orchardHelper.HttpContext.RequestServices.GetService<ViewContextAccessor>();
            var contentManager = orchardHelper.HttpContext.RequestServices.GetService<IContentManager>();

            var urlHelper = urlHelperFactory.GetUrlHelper(viewContextAccessor.ViewContext);
            var metadata = await contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);
            return
                $"<a href='{urlHelper.Action(metadata.EditorRouteValues["action"].ToString(), metadata.EditorRouteValues)}'>{linkText}</a>";
        }
    }
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Contents.AuditTrail.Extensions
{
    public static class OrchardHtmlExtensions
    {
        public static async Task<string> GetItemEditLinkAsync(this IOrchardHelper orchardHelper, string linkText, ContentItem contentItem)
        {
            var urlHelperFactory = orchardHelper.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
            var viewContextAccessor = orchardHelper.HttpContext.RequestServices.GetRequiredService<ViewContextAccessor>();
            var contentManager = orchardHelper.HttpContext.RequestServices.GetRequiredService<IContentManager>();

            var urlHelper = urlHelperFactory.GetUrlHelper(viewContextAccessor.ViewContext);
            var metadata = await contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);

            return $"<a href='{urlHelper.Action(metadata.EditorRouteValues["action"].ToString(), metadata.EditorRouteValues)}'>{linkText}</a>";
        }
    }
}

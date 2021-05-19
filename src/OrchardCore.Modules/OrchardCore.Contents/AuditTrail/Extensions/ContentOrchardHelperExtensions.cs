using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Contents.AuditTrail.Extensions
{
    public static class ContentOrchardHelperExtensions
    {
        public static async Task<HtmlString> EditForLinkAsync(this IOrchardHelper orchardHelper, string linkText, ContentItem contentItem)
        {
            var urlHelperFactory = orchardHelper.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
            var viewContextAccessor = orchardHelper.HttpContext.RequestServices.GetRequiredService<ViewContextAccessor>();
            var contentManager = orchardHelper.HttpContext.RequestServices.GetRequiredService<IContentManager>();
            var htmlEncoder = orchardHelper.HttpContext.RequestServices.GetRequiredService<HtmlEncoder>();

            var urlHelper = urlHelperFactory.GetUrlHelper(viewContextAccessor.ViewContext);
            var metadata = await contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);
            var action = urlHelper.Action(metadata.EditorRouteValues["action"].ToString(), metadata.EditorRouteValues);
            var link = htmlEncoder.Encode(linkText ?? "");

            return new HtmlString($"<a href='{action}'>{link}</a>");
        }
    }
}

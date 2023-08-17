using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Contents.AuditTrail.Extensions
{
    public static class ContentOrchardHelperExtensions
    {
        public static async Task<IHtmlContent> EditForLinkAsync(this IOrchardHelper orchardHelper, string linkText, ContentItem contentItem)
        {
            var viewContextAccessor = orchardHelper.HttpContext.RequestServices.GetRequiredService<ViewContextAccessor>();
            var viewContext = viewContextAccessor.ViewContext;
            var helper = MakeHtmlHelper(viewContext, viewContext.ViewData);
            var contentManager = orchardHelper.HttpContext.RequestServices.GetRequiredService<IContentManager>();
            var metadata = await contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);

            if (String.IsNullOrEmpty(linkText))
            {
                linkText = contentItem.ContentType;
            }

            return helper.ActionLink(linkText, metadata.EditorRouteValues["action"].ToString(), metadata.EditorRouteValues);
        }

        private static IHtmlHelper MakeHtmlHelper(ViewContext viewContext, ViewDataDictionary viewData)
        {
            var newHelper = viewContext.HttpContext.RequestServices.GetRequiredService<IHtmlHelper>();

            var contextable = newHelper as IViewContextAware;
            if (contextable != null)
            {
                var newViewContext = new ViewContext(viewContext, viewContext.View, viewData, viewContext.Writer);
                contextable.Contextualize(newViewContext);
            }

            return newHelper;
        }
    }
}

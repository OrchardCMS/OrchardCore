using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentLocalization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.ContentFields
{
    public static class ContentItemDisplayExtensions
    {
        public static async Task<string> GetContentItemRouteAndCulture(this ContentItem contentItem, IContentManager contentManager, IStringLocalizer S)
        {
            var displayText = string.Empty;
            var additionalParts = new List<string>();

            // add culture to display text
            var cultureAspect = await contentManager.PopulateAspectAsync<CultureAspect>(contentItem);
            if (cultureAspect.HasCulture)
            {
                additionalParts.Add($"{S["Culture"]}: {cultureAspect.Culture}");
            }

            // add autoroute path to display text
            var routeAspect = await contentManager.PopulateAspectAsync<RouteHandlerAspect>(contentItem);
            if (!routeAspect.Disabled)
            {
                additionalParts.Add($"{S["Route"]}: {routeAspect.Path.Replace(contentItem.ContentItemId + "-", string.Empty)}");
            }

            if (additionalParts.Count > 0)
            {
                displayText += $" ({string.Join(", ", additionalParts)})";
            }

            return displayText;
        }
    }
}

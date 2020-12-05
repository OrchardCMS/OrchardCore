using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.TagHelpers;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Admin.TagHelpers
{
    [HtmlTargetElement("titleZone")]
    public class TitleZoneTagHelper : ZoneTagHelper
    {
        private const string ContentZoneName = "Content";
        private const string TitleZoneName = "Title";

        private readonly ILayoutAccessor _layoutAccessor;
        private readonly ISiteService _siteService;

        public TitleZoneTagHelper(ILayoutAccessor layoutAccessor, ISiteService siteService) : base(layoutAccessor)
        {
            _layoutAccessor = layoutAccessor;
            _siteService = siteService;
        }

        public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var adminSettings = (await _siteService.GetSiteSettingsAsync()).As<AdminSettings>();
            Name = adminSettings.DisplayTitlesInTopbar ? TitleZoneName : ContentZoneName;

            var childContent = await output.GetChildContentAsync();
            dynamic layout = await _layoutAccessor.GetLayoutAsync();
            var zone = layout.Zones[Name];

            if (zone is ZoneOnDemand zoneOnDemand)
            {
                await zoneOnDemand.AddAsync(childContent, Position);
            }
            else if (zone is Shape shape)
            {
                shape.Add(childContent, Position);
            }

            // Don't render the zone tag or the inner content
            output.SuppressOutput();
        }
    }
}

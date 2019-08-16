using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("zone", Attributes = NameAttribute)]
    public class ZoneTagHelper : TagHelper
    {
        private const string PositionAttribute = "position";
        private const string NameAttribute = "name";

        private readonly ILayoutAccessor _layoutAccessor;

        public ZoneTagHelper(ILayoutAccessor layoutAccessor)
        {
            _layoutAccessor = layoutAccessor;
        }

        [HtmlAttributeName(PositionAttribute)]
        public string Position { get; set; }

        [HtmlAttributeName(NameAttribute)]
        public string Name { get; set; }

        public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (String.IsNullOrEmpty(Name))
            {
                throw new ArgumentException("The name attribute can't be empty");
            }

            var childContent = await output.GetChildContentAsync();
            dynamic layout = await _layoutAccessor.GetLayoutAsync();
            var zone = layout.Zones[Name];

            if (zone is ZoneOnDemand zoneOnDemand)
            {
                await zoneOnDemand.AddAsync(childContent, Position);
            }
            else
            {
                zone.Add(childContent, Position);
            }

            // Don't render the zone tag or the inner content
            output.SuppressOutput();
        }
    }
}

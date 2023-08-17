using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("zone", Attributes = NameAttribute)]
    public class ZoneTagHelper : TagHelper
    {
        private const string PositionAttribute = "position";
        private const string NameAttribute = "name";

        private readonly ILayoutAccessor _layoutAccessor;
        private readonly ILogger _logger;

        public ZoneTagHelper(ILayoutAccessor layoutAccessor, ILogger<ZoneTagHelper> logger)
        {
            _layoutAccessor = layoutAccessor;
            _logger = logger;
        }

        [HtmlAttributeName(PositionAttribute)]
        public string Position { get; set; }

        [HtmlAttributeName(NameAttribute)]
        public string Name { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (String.IsNullOrEmpty(Name))
            {
                throw new ArgumentException("The name attribute can't be empty");
            }

            var childContent = await output.GetChildContentAsync();
            var layout = await _layoutAccessor.GetLayoutAsync();

            var zone = layout.Zones[Name];

            if (zone is Shape shape)
            {
                await shape.AddAsync(childContent, Position);
            }
            else
            {
                _logger.LogWarning(
                    "Unable to add shape to the zone using the <zone> tag helper because the zone's type is " +
                    "\"{ActualType}\" instead of the expected {ExpectedType}",
                    zone.GetType().FullName,
                    nameof(Shape));
            }

            // Don't render the zone tag or the inner content
            output.SuppressOutput();
        }
    }
}

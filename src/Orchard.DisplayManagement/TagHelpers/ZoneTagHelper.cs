using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.Extensions.WebEncoders;
using Orchard.DisplayManagement.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("zone", Attributes = NameAttribute)]
    public class MarkdownTagHelper : TagHelper
    {
        private const string PositionAttribute = "position";
        private const string NameAttribute = "name";

        private readonly ILayoutAccessor _layoutAccessor;

        public MarkdownTagHelper(ILayoutAccessor layoutAccessor)
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
            var zone = _layoutAccessor.GetLayout().Zones[Name];

            zone.Add(childContent, Position);

            // Don't render the zone tag or the inner content
            output.SuppressOutput();
        }
    }
}

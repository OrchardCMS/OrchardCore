using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement(TagName, TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ShapeMetadataTagHelper : TagHelper
    {
        internal const string TagName = "metadata";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var shape = (IShape)context.Items[typeof(IShape)];

            if (shape != null && output.Attributes.ContainsName("display-type"))
            {
                shape.Metadata.DisplayType = Convert.ToString(output.Attributes["display-type"].Value);
            }

            await output.GetChildContentAsync();
            output.SuppressOutput();
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement(TagName, TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ShapeMetadataTagHelper : TagHelper
    {
        internal const string TagName = "metadata";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var metadata = (ShapeMetadata)context.Items[typeof(ShapeMetadata)];

            if (metadata != null && output.Attributes.ContainsName("display-type"))
            {
                metadata.DisplayType = Convert.ToString(output.Attributes["display-type"].Value);
            }

            await output.GetChildContentAsync();
            output.SuppressOutput();
        }
    }
}
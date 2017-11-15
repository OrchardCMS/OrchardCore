using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("remove-alternate", ParentTag = ShapeMetadataTagHelper.TagName, Attributes = "name", TagStructure = TagStructure.WithoutEndTag)]
    public class RemoveAlternateTagHelper : TagHelper
    {
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var metadata = (ShapeMetadata)context.Items[typeof(ShapeMetadata)];

            metadata?.Alternates.Remove(Convert.ToString(output.Attributes["name"].Value));

            output.SuppressOutput();

            return Task.CompletedTask;
        }
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("add-alternate", ParentTag = ShapeMetadataTagHelper.TagName, Attributes = "name", TagStructure = TagStructure.WithoutEndTag)]
    public class AddAlternateTagHelper : TagHelper
    {
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var metadata = (ShapeMetadata)context.Items[typeof(ShapeMetadata)];

            metadata?.Alternates.Add(Convert.ToString(output.Attributes["name"].Value));

            output.SuppressOutput();

            return Task.CompletedTask;
        }
    }
}
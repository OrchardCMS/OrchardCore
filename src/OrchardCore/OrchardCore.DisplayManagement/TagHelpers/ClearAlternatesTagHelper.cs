using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("clear-alternates", ParentTag = ShapeMetadataTagHelper.TagName, TagStructure = TagStructure.WithoutEndTag)]
    public class ClearAlternatesTagHelper : TagHelper
    {
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var metadata = (ShapeMetadata)context.Items[typeof(ShapeMetadata)];

            metadata?.Alternates.Clear();

            output.SuppressOutput();

            return Task.CompletedTask;
        }
    }
}
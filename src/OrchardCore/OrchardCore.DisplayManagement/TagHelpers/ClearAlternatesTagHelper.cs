using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("clear-alternates", ParentTag = ShapeMetadataTagHelper.TagName, TagStructure = TagStructure.WithoutEndTag)]
    public class ClearAlternatesTagHelper : TagHelper
    {
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var shape = (IShape)context.Items[typeof(IShape)];

            shape?.Metadata.Alternates.Clear();

            output.SuppressOutput();

            return Task.CompletedTask;
        }
    }
}

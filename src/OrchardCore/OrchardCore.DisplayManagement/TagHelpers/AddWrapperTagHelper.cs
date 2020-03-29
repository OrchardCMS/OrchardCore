using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("add-wrapper", ParentTag = ShapeMetadataTagHelper.TagName, Attributes = "name", TagStructure = TagStructure.WithoutEndTag)]
    public class AddWrapperTagHelper : TagHelper
    {
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var shape = (IShape)context.Items[typeof(IShape)];

            shape?.Metadata.Wrappers.Add(Convert.ToString(output.Attributes["name"].Value));

            output.SuppressOutput();

            return Task.CompletedTask;
        }
    }
}

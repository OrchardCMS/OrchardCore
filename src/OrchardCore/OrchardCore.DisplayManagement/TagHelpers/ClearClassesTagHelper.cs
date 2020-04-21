using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("clear-classes", TagStructure = TagStructure.WithoutEndTag)]
    public class ClearClassesTagHelper : TagHelper
    {
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var shape = (IShape)context.Items[typeof(IShape)];

            shape?.Classes.Clear();

            output.SuppressOutput();

            return Task.CompletedTask;
        }
    }
}

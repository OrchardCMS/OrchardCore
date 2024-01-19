using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.TagHelpers;

[HtmlTargetElement("html-content", Attributes = "name", TagStructure = TagStructure.NormalOrSelfClosing)]
public class HtmlContentTagHelper : TagHelper
{
    [HtmlAttributeName("name")]
    public string Name { get; set; }
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (context.Items[typeof(IShape)] is IShape shape && !string.IsNullOrWhiteSpace(Name))
        {
            var content = await output.GetChildContentAsync(useCachedResult: false);
            shape.Properties[Name.Trim()] = new HtmlString(content.GetContent());
        }

        output.SuppressOutput();
    }
}

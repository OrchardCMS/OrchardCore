using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.TagHelpers;

[HtmlTargetElement("add-property", Attributes = "name", TagStructure = TagStructure.NormalOrSelfClosing)]
public class HtmlContentTagHelper : TagHelper
{
    [HtmlAttributeName("name")]
    public string Name { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var shape = (IShape)context.Items[typeof(IShape)];

        if (!string.IsNullOrWhiteSpace(Name))
        {
            var content = await output.GetChildContentAsync(useCachedResult: false);
            shape.Properties[Name.Trim()] = new HtmlString(content.GetContent());
        }

        output.SuppressOutput();
    }
}

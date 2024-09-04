using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers;

[HtmlTargetElement("add-property", Attributes = "name", TagStructure = TagStructure.NormalOrSelfClosing)]
public class AddPropertyTagHelper : TagHelper
{
    [HtmlAttributeName("name")]
    public string Name { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            return;
        }

        var content = await output.GetChildContentAsync(useCachedResult: false);
        var shape = (IShape)context.Items[typeof(IShape)];
        shape.Properties[Name.Trim()] = output.Attributes.ContainsName("value")
            ? output.Attributes["value"].Value
            : new HtmlString(content.GetContent());

        output.SuppressOutput();
    }
}

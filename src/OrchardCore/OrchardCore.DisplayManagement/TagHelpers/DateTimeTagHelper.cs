using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers;

[HtmlTargetElement("datetime")]
public class DateTimeTagHelper : TagHelper
{
    private const string UtcAttribute = "utc";
    private const string FormatAttribute = "format";

    protected IShapeFactory _shapeFactory;
    protected IDisplayHelper _displayHelper;

    public DateTimeTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelper)
    {
        _shapeFactory = shapeFactory;
        _displayHelper = displayHelper;
    }

    [HtmlAttributeName(UtcAttribute)]
    public DateTime? Utc { set; get; }

    [HtmlAttributeName(FormatAttribute)]
    public string Format { set; get; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var shapeType = "DateTime";
        var shape = await _shapeFactory.CreateAsync(shapeType);
        shape.Properties["Utc"] = Utc;
        shape.Properties["Format"] = Format;

        output.Content.SetHtmlContent(await _displayHelper.ShapeExecuteAsync(shape));

        // We don't want any encapsulating tag around the shape
        output.TagName = null;
    }
}

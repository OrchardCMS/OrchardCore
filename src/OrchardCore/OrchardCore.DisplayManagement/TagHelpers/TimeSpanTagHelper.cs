using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers;

[HtmlTargetElement("timespan")]
public class TimeSpanTagHelper : TagHelper
{
    private const string UtcAttribute = "utc";
    private const string OriginAttribute = "origin";

    protected IShapeFactory _shapeFactory;
    protected IDisplayHelper _displayHelper;

    public TimeSpanTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelper)
    {
        _shapeFactory = shapeFactory;
        _displayHelper = displayHelper;
    }

    [HtmlAttributeName(UtcAttribute)]
    public DateTime? Utc { set; get; }

    [HtmlAttributeName(OriginAttribute)]
    public DateTime? Origin { set; get; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var shapeType = "TimeSpan";
        var shape = await _shapeFactory.CreateAsync(shapeType);
        shape.Properties["Utc"] = Utc;
        shape.Properties["Origin"] = Origin;

        output.Content.SetHtmlContent(await _displayHelper.ShapeExecuteAsync(shape));

        // We don't want any encapsulating tag around the shape
        output.TagName = null;
    }
}

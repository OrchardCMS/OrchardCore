using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Admin.TagHelpers;

/// <summary>
/// Displays a cron expression with a description of when it occurs, such as "At 03:00 AM", through the
/// <c>CronExpression</c> shape.
/// </summary>
[HtmlTargetElement("cron-expression", Attributes = "value")]
public sealed class CronExpressionTagHelper : TagHelper
{
    /// <summary>
    /// The type of the shape that displays a cron expression.
    /// </summary>
    public const string ShapeType = "CronExpression";

    private readonly IDisplayHelper _displayHelper;
    private readonly IShapeFactory _shapeFactory;

    /// <summary>
    /// Creates a helper that renders through the standard shape pipeline.
    /// </summary>
    /// <param name="displayHelper">The shape display helper.</param>
    /// <param name="shapeFactory">The shape factory.</param>
    public CronExpressionTagHelper(IDisplayHelper displayHelper, IShapeFactory shapeFactory)
    {
        _displayHelper = displayHelper;
        _shapeFactory = shapeFactory;
    }

    /// <summary>
    /// The cron expression, with five fields.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Where the description of the expression is shown. It is shown as text by default.
    /// </summary>
    public CronExpressionDescription Description { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var shape = await _shapeFactory.CreateAsync(ShapeType, Arguments.From(new
        {
            Expression = Value,
            Description,
        }));

        output.TagName = null;
        output.Attributes.Clear();
        output.Content.SetHtmlContent(await _displayHelper.ShapeExecuteAsync(shape));
    }
}

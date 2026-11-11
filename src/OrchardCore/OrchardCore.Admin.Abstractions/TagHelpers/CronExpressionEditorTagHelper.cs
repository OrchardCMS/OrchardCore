using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Admin.TagHelpers;

/// <summary>
/// Renders the editor of a cron expression through the <c>CronExpressionEditor</c> shape: its text box, a menu of
/// common schedules, a link to crontab.guru, and a description of the expression with its next occurrences that
/// follows what is typed. The other attributes of the tag are added to the text box.
/// </summary>
[HtmlTargetElement("cron-expression-editor", Attributes = ForAttributeName)]
public sealed class CronExpressionEditorTagHelper : TagHelper
{
    /// <summary>
    /// The type of the shape that edits a cron expression.
    /// </summary>
    public const string ShapeType = "CronExpressionEditor";

    private const string ForAttributeName = "asp-for";
    private const string SiteTimeZoneForAttributeName = "site-time-zone-for";

    private readonly IDisplayHelper _displayHelper;
    private readonly IShapeFactory _shapeFactory;
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    /// Creates a helper that renders through the standard shape pipeline.
    /// </summary>
    /// <param name="displayHelper">The shape display helper.</param>
    /// <param name="shapeFactory">The shape factory.</param>
    /// <param name="htmlGenerator">The generator of the text box.</param>
    public CronExpressionEditorTagHelper(IDisplayHelper displayHelper, IShapeFactory shapeFactory, IHtmlGenerator htmlGenerator)
    {
        _displayHelper = displayHelper;
        _shapeFactory = shapeFactory;
        _htmlGenerator = htmlGenerator;
    }

    /// <summary>
    /// The property of the model that holds the cron expression.
    /// </summary>
    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; }

    /// <summary>
    /// The cron expression the menu of common schedules offers as the default one, when there is one.
    /// </summary>
    public string DefaultValue { get; set; }

    /// <summary>
    /// Whether the expression applies to UTC. It applies to the time zone of the site otherwise, which is the default.
    /// </summary>
    public bool Utc { get; set; }

    /// <summary>
    /// The boolean property of the model, edited by a checkbox of the same form, that makes the expression apply to
    /// the time zone of the site when it is checked, for an expression that applies to UTC otherwise.
    /// </summary>
    [HtmlAttributeName(SiteTimeZoneForAttributeName)]
    public ModelExpression SiteTimeZoneFor { get; set; }

    /// <summary>
    /// The context of the view.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(For);

        var attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["spellcheck"] = "false",
            ["autocomplete"] = "off",
        };

        // The attributes of the tag that are not its own are the ones of the text box.
        foreach (var attribute in output.Attributes)
        {
            attributes[attribute.Name] = attribute.Value;
        }

        attributes["class"] = attributes.TryGetValue("class", out var cssClass)
            ? $"form-control font-monospace {cssClass}"
            : "form-control font-monospace";
        attributes["data-cron-input"] = "true";

        var input = _htmlGenerator.GenerateTextBox(ViewContext, For.ModelExplorer, For.Name, For.Model, format: null, attributes);
        input.TagRenderMode = TagRenderMode.SelfClosing;

        string siteTimeZoneToggleId = null;

        if (SiteTimeZoneFor is not null)
        {
            var name = ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(SiteTimeZoneFor.Name);
            siteTimeZoneToggleId = TagBuilder.CreateSanitizedId(name, _htmlGenerator.IdAttributeDotReplacement);
        }

        input.Attributes.TryGetValue("id", out var inputId);
        input.Attributes.TryGetValue("value", out var value);

        var shape = await _shapeFactory.CreateAsync(ShapeType, Arguments.From(new
        {
            Input = input,
            InputId = inputId,
            Value = value,
            DefaultValue,
            Utc,
            SiteTimeZoneToggleId = siteTimeZoneToggleId,
        }));

        output.TagName = null;
        output.Attributes.Clear();
        output.Content.SetHtmlContent(await _displayHelper.ShapeExecuteAsync(shape));
    }
}

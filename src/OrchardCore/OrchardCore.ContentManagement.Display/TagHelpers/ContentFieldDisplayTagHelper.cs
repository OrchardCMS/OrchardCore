using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement;

namespace OrchardCore.ContentManagement.Display.TagHelpers;

/// <summary>
/// Renders the value of a content field, given as the child content, according to the display settings of the field:
/// the field isn't rendered when it has no value, unless its <see cref="ContentPartFieldSettings.AlwaysDisplay"/>
/// setting is enabled, and its name is rendered with the <c>ContentFieldDisplay</c> shape when its
/// <see cref="ContentPartFieldSettings.DisplayFieldName"/> setting is enabled.
/// </summary>
/// <example>
/// <code>
/// &lt;content-field-display definition="Model.PartFieldDefinition" has-value="!string.IsNullOrWhiteSpace(Model.Text)"&gt;
///     &lt;div class="field"&gt;@Model.Text&lt;/div&gt;
/// &lt;/content-field-display&gt;
/// </code>
/// </example>
[HtmlTargetElement(TagName, Attributes = DefinitionAttributeName)]
public sealed class ContentFieldDisplayTagHelper : TagHelper
{
    /// <summary>
    /// The shape rendering the name of the field next to its value.
    /// </summary>
    public const string ShapeType = "ContentFieldDisplay";

    private const string TagName = "content-field-display";
    private const string DefinitionAttributeName = "definition";

    private readonly IShapeFactory _shapeFactory;
    private readonly IDisplayHelper _displayHelper;

    public ContentFieldDisplayTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelper)
    {
        _shapeFactory = shapeFactory;
        _displayHelper = displayHelper;
    }

    /// <summary>
    /// Gets or sets the definition of the displayed field.
    /// </summary>
    [HtmlAttributeName(DefinitionAttributeName)]
    public ContentPartFieldDefinition Definition { get; set; }

    /// <summary>
    /// Gets or sets whether the field has a value to display. Defaults to <see langword="true"/>.
    /// </summary>
    [HtmlAttributeName("has-value")]
    public bool HasValue { get; set; } = true;

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;

        var settings = Definition.GetSettings<ContentPartFieldSettings>();

        if (!HasValue && !settings.AlwaysDisplay)
        {
            output.SuppressOutput();

            return;
        }

        var content = await output.GetChildContentAsync();

        if (!settings.DisplayFieldName)
        {
            output.Content.SetHtmlContent(content);

            return;
        }

        var shape = await _shapeFactory.CreateAsync(ShapeType, Arguments.From(new Dictionary<string, object>
        {
            ["PartFieldDefinition"] = Definition,
            ["Content"] = content,
        }));

        // The display type of the field shape, for instance to render a field without its name in a grid column.
        if (ViewContext?.ViewData.Model is IShape fieldShape)
        {
            shape.Metadata.DisplayType = fieldShape.Metadata.DisplayType;
        }

        output.Content.SetHtmlContent(await _displayHelper.ShapeExecuteAsync(shape));
    }
}

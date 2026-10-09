using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Secrets.TagHelpers;

/// <summary>
/// Renders the editor of a credential bound to a <see cref="SecretInputViewModel"/>.
/// </summary>
/// <remarks>
/// Without the Secrets feature, this renders a password input whose value is kept, protected, in the settings. When the
/// Secrets feature is enabled, this renders its <c>SecretInput</c> partial view, which also lets the user reference a secret.
/// </remarks>
[HtmlTargetElement("secret-input", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
public sealed class SecretInputTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    /// <summary>
    /// The name of the partial view provided by the Secrets module.
    /// </summary>
    public const string PartialViewName = "SecretInput";

    private readonly IHtmlHelper _htmlHelper;

    internal readonly IStringLocalizer S;

    public SecretInputTagHelper(
        IHtmlHelper htmlHelper,
        IStringLocalizer<SecretInputTagHelper> stringLocalizer)
    {
        _htmlHelper = htmlHelper;
        S = stringLocalizer;
    }

    /// <summary>
    /// Gets or sets the <see cref="SecretInputViewModel"/> property to render.
    /// </summary>
    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; }

    /// <summary>
    /// Gets or sets the name suggested when the user creates a secret for this credential, for instance <c>Facebook.AppSecret</c>.
    /// </summary>
    [HtmlAttributeName("secret-name")]
    public string SuggestedSecretName { get; set; }

    /// <summary>
    /// Gets or sets the placeholder of the value input.
    /// </summary>
    [HtmlAttributeName("placeholder")]
    public string Placeholder { get; set; }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var htmlName = ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(For.Name);

        var model = new SecretInputEditorViewModel
        {
            Input = For.Model as SecretInputViewModel ?? new SecretInputViewModel(),
            HtmlName = htmlName,
            HtmlId = TagBuilder.CreateSanitizedId(htmlName, _htmlHelper.IdAttributeDotReplacement),
            SuggestedSecretName = SuggestedSecretName,
            Placeholder = Placeholder,
        };

        output.TagName = null;
        output.TagMode = TagMode.StartTagAndEndTag;

        // The Secrets module provides a richer editor when it is enabled.
        if (ViewContext.HttpContext.RequestServices.GetService<ISecretManager>() is not null)
        {
            (_htmlHelper as IViewContextAware)?.Contextualize(ViewContext);

            output.Content.SetHtmlContent(await _htmlHelper.PartialAsync(PartialViewName, model));

            return;
        }

        output.Content.SetHtmlContent(RenderValueInput(model));
    }

    private TagBuilder RenderValueInput(SecretInputEditorViewModel model)
    {
        var input = new TagBuilder("input")
        {
            TagRenderMode = TagRenderMode.SelfClosing,
        };

        input.Attributes["type"] = "password";
        input.Attributes["id"] = model.HtmlId;
        input.Attributes["name"] = model.HtmlName + "." + nameof(SecretInputViewModel.Value);
        input.Attributes["autocomplete"] = "new-password";
        input.AddCssClass("form-control");

        input.Attributes["placeholder"] = model.Input.HasValue
            ? S["The value is securely stored. Enter a new value to update it, or leave it blank to keep it."].Value
            : model.Placeholder ?? S["Enter a new value"].Value;

        if (string.IsNullOrWhiteSpace(model.Input.SecretName))
        {
            return input;
        }

        // A secret was referenced while the Secrets feature was enabled. It is kept until a value is entered.
        var container = new TagBuilder("div");
        container.InnerHtml.AppendHtml(input);

        var alert = new TagBuilder("div");
        alert.AddCssClass("alert alert-warning mt-2 mb-0");
        alert.Attributes["role"] = "alert";
        alert.InnerHtml.Append(S["This setting references the secret '{0}', but the Secrets feature is not enabled. Enable it, or enter a value to keep in these settings.", model.Input.SecretName].Value);
        container.InnerHtml.AppendHtml(alert);

        return container;
    }
}

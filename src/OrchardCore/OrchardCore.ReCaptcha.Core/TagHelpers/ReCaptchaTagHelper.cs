using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.TagHelpers;

namespace OrchardCore.ReCaptcha.TagHelpers;

[HtmlTargetElement("captcha", TagStructure = TagStructure.WithoutEndTag)]
[HtmlTargetElement("captcha", Attributes = "language,onload", TagStructure = TagStructure.WithoutEndTag)]
public sealed class ReCaptchaTagHelper : BaseShapeTagHelper
{
    public ReCaptchaTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelper)
        : base(shapeFactory, displayHelper)
    {
        Type = "ReCaptcha";
    }

    /// <summary>
    /// The two letter ISO code of the language the captcha should be displayed in.
    /// When left blank it will fall back to the default OrchardCore language.
    /// </summary>
    [HtmlAttributeName("language")]
    public string Language { get; set; }

    /// <summary>
    /// The name of the JavaScript callback method to be called when the reCAPTCHA loads.
    /// </summary>
    [HtmlAttributeName("onload")]
    public string OnLoad { get; set; }

    public override Task ProcessAsync(TagHelperContext tagHelperContext, TagHelperOutput output)
    {
        if (!string.IsNullOrWhiteSpace(Language))
        {
            output.Attributes.Add("language", Language);
        }

        if (!string.IsNullOrWhiteSpace(OnLoad))
        {
            output.Attributes.Add("onload", OnLoad);
        }

        return base.ProcessAsync(tagHelperContext, output);
    }
}

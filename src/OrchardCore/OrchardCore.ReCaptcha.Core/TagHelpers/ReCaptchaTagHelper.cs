using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.TagHelpers;
using OrchardCore.ReCaptcha.ActionFilters;

namespace OrchardCore.ReCaptcha.TagHelpers;

[HtmlTargetElement("captcha", TagStructure = TagStructure.WithoutEndTag)]
[HtmlTargetElement("captcha", Attributes = "mode,language,onload", TagStructure = TagStructure.WithoutEndTag)]
public class ReCaptchaTagHelper : BaseShapeTagHelper
{
    public ReCaptchaTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelper)
        : base(shapeFactory, displayHelper)
    {
        Type = "ReCaptcha";
        Mode = ReCaptchaMode.PreventRobots;
    }

    [HtmlAttributeName("mode")]
    public ReCaptchaMode Mode { get; set; }

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
        output.Attributes.Add("mode", Mode);

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

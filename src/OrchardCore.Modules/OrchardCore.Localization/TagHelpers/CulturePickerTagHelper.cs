using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Localization.TagHelpers
{
    [HtmlTargetElement("culture-picker", TagStructure = TagStructure.WithoutEndTag)]
    public class CulturePickerTagHelper : TagHelper
    {
        private readonly ISiteService _siteService;
        private readonly IResourceManager _resourceManager;

        public CulturePickerTagHelper(ISiteService siteService, IResourceManager resourceManager)
        {
            _siteService = siteService;
            _resourceManager = resourceManager;
        }

        [ViewContext, HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            var selectedCulture = ViewContext.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var selectTag = new TagBuilder("select");
            selectTag.AddCssClass("form-control");
            selectTag.Attributes.Add("onchange", "setLocalizationCookie(this.value))");

            foreach (var culture in siteSettings.SupportedCultures)
            {
                var optionTag = new TagBuilder("option");
                optionTag.Attributes.Add("value", culture);

                if (culture == selectedCulture)
                {
                    optionTag.Attributes.Add("selected", "selected");
                }

                optionTag.InnerHtml.Append(CultureInfo.GetCultureInfo(culture).DisplayName);
                selectTag.InnerHtml.AppendHtml(optionTag);
            }

            output.Content.AppendHtml(selectTag);
            AppendLocalizationCookieScript();
        }


        private void AppendLocalizationCookieScript()
        {
            var localizationScript = new TagBuilder("script");
            localizationScript.InnerHtml.AppendHtml($@"function setLocalizationCookie(code) {{
        document.cookie = '{CookieRequestCultureProvider.DefaultCookieName}=c='+code+'|uic='+code;
        window.location.reload();
    }}");
            _resourceManager.RegisterFootScript(localizationScript);
        }
    }
}
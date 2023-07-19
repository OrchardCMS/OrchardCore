using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.ReCaptcha.ActionFilters;
using OrchardCore.ReCaptcha.ActionFilters.Detection;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.ResourceManagement;

namespace OrchardCore.ReCaptcha.TagHelpers
{
    [HtmlTargetElement("captcha", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("captcha", Attributes = "mode,language", TagStructure = TagStructure.WithoutEndTag)]
    public class ReCaptchaTagHelper : TagHelper
    {
        private readonly IResourceManager _resourceManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ReCaptchaSettings _settings;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;

        public ReCaptchaTagHelper(
            IOptions<ReCaptchaSettings> optionsAccessor,
            IResourceManager resourceManager,
            ILocalizationService localizationService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ReCaptchaTagHelper> logger)
        {
            _resourceManager = resourceManager;
            _httpContextAccessor = httpContextAccessor;
            _settings = optionsAccessor.Value;
            Mode = ReCaptchaMode.PreventRobots;
            _localizationService = localizationService;
            _logger = logger;
        }

        [HtmlAttributeName("mode")]
        public ReCaptchaMode Mode { get; set; }

        /// <summary>
        /// The two letter ISO code of the language the captcha should be displayed in.
        /// When left blank it will fall back to the default OrchardCore language.
        /// </summary>
        [HtmlAttributeName("language")]
        public string Language { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var robotDetectors = _httpContextAccessor.HttpContext.RequestServices.GetServices<IDetectRobots>();
            var robotDetected = robotDetectors.Invoke(d => d.DetectRobot(), _logger).Any(d => d.IsRobot) && Mode == ReCaptchaMode.PreventRobots;
            var alwaysShow = Mode == ReCaptchaMode.AlwaysShow;
            var isConfigured = _settings != null;

            if (isConfigured && (robotDetected || alwaysShow))
            {
                await ShowCaptcha(output);
            }
            else
            {
                output.SuppressOutput();
            }
        }

        private async Task ShowCaptcha(TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "g-recaptcha");
            output.Attributes.SetAttribute("data-sitekey", _settings.SiteKey);
            output.TagMode = TagMode.StartTagAndEndTag;

            var builder = new TagBuilder("script");
            var cultureInfo = await GetCultureAsync();

            var settingsUrl = $"{_settings.ReCaptchaScriptUri}?hl={cultureInfo.TwoLetterISOLanguageName}";

            builder.Attributes.Add("src", settingsUrl);
            _resourceManager.RegisterFootScript(builder);
        }

        private async Task<CultureInfo> GetCultureAsync()
        {
            var language = Language;
            CultureInfo culture = null;

            if (String.IsNullOrWhiteSpace(language))
            {
                language = await _localizationService.GetDefaultCultureAsync();
            }

            try
            {
                culture = CultureInfo.GetCultureInfo(language);
            }
            catch (CultureNotFoundException)
            {
                _logger.LogWarning("Language with name {LanguageName} not found.", language);
            }

            return culture;
        }
    }
}

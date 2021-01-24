using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Localization;
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
        private readonly IEnumerable<IDetectRobots> _robotDetectors;
        private readonly ReCaptchaSettings _settings;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly IStringLocalizer<ReCaptchaTagHelper> S;

        public ReCaptchaTagHelper(
            IOptions<ReCaptchaSettings> optionsAccessor,
            IResourceManager resourceManager,
            ILocalizationService localizationService,
            IEnumerable<IDetectRobots> robotDetectors,
            ILogger<ReCaptchaTagHelper> logger,
            IStringLocalizer<ReCaptchaTagHelper> localizer)
        {
            _resourceManager = resourceManager;
            _robotDetectors = robotDetectors;
            _settings = optionsAccessor.Value;
            Mode = ReCaptchaMode.PreventRobots;
            _logger = logger;
            _localizationService = localizationService;
            S = localizer;
        }

        [HtmlAttributeName("mode")]
        public ReCaptchaMode Mode { get; set; }

        /// <summary>
        /// The two letter ISO code of the language the captcha should be displayed in
        /// When left blank it will fall back to the default OrchardCore language
        /// </summary>
        [HtmlAttributeName("language")]
        public string Language { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            void RenderDivToTagHelper(TagBuilder builder)
            {
                output.TagName = builder.TagName;
                output.MergeAttributes(builder);
                output.TagMode = TagMode.StartTagAndEndTag;
            }

            void DoNotRenderTagHelper()
            {
                output.SuppressOutput();
            }

            await ReCaptchaRenderer.ShowCaptchaOrCallback(
                _settings, Mode, Language, _robotDetectors, _localizationService, _resourceManager, S, _logger,
                RenderDivToTagHelper, DoNotRenderTagHelper);
        }
    }
}

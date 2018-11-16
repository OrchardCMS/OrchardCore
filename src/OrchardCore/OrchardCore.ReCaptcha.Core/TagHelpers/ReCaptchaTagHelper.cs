using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.ReCaptcha.ActionFilters;
using OrchardCore.ReCaptcha.ActionFilters.Abuse;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.ResourceManagement;

namespace OrchardCore.ReCaptcha.TagHelpers
{
    [HtmlTargetElement("captcha", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("captcha", Attributes = "mode", TagStructure = TagStructure.WithoutEndTag)]
    public class ReCaptchaTagHelper : TagHelper
    {
        private readonly IResourceManager _resourceManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ReCaptchaSettings _settings;
        private readonly ILogger<ReCaptchaTagHelper> _logger;

        public ReCaptchaTagHelper(IOptions<ReCaptchaSettings> optionsAccessor, IResourceManager resourceManager, IHttpContextAccessor httpContextAccessor, ILogger<ReCaptchaTagHelper> logger)
        {
            _resourceManager = resourceManager;
            _httpContextAccessor = httpContextAccessor;
            _settings = optionsAccessor.Value;
            Mode = ReCaptchaMode.PreventRobots;
            _logger = logger;
        }

        [HtmlAttributeName("mode")]
        public ReCaptchaMode Mode { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var robotDetectors = _httpContextAccessor.HttpContext.RequestServices.GetServices<IDetectRobots>();
            var robotDetected = robotDetectors.Invoke(d => d.DetectRobot(), _logger).Any(d => d.IsRobot) && Mode == ReCaptchaMode.PreventRobots;
            var alwaysShow = Mode == ReCaptchaMode.AlwaysShow;
            var isConfigured = _settings != null;

            if (isConfigured && (robotDetected || alwaysShow))
            {
                ShowCaptcha(output);
            }
            else
            {
                output.SuppressOutput();
            }
        }

        private void ShowCaptcha(TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "g-recaptcha");
            output.Attributes.SetAttribute("data-sitekey", _settings.SiteKey);
            output.TagMode = TagMode.StartTagAndEndTag;

            var builder = new TagBuilder("script");
            builder.Attributes.Add("src", _settings.ReCaptchaScriptUri);
            _resourceManager.RegisterFootScript(builder);
        }
    }
}

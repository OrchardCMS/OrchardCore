using System.Linq;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.ReCaptcha.Core.ActionFilters;
using OrchardCore.ReCaptcha.Core.ActionFilters.Abuse;
using OrchardCore.ReCaptcha.Core.Configuration;
using OrchardCore.ResourceManagement;

namespace OrchardCore.ReCaptcha.Core.TagHelpers
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
            Mode = ReCaptchaMode.PreventAbuse;
            _logger = logger;
        }

        [HtmlAttributeName("mode")]
        public ReCaptchaMode Mode { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var abuse = _httpContextAccessor.HttpContext.RequestServices.GetServices<IDetectAbuse>();
            var abuseDetected = abuse.Invoke(d => d.DetectAbuse(httpContext), _logger).Any(d => d.SuspectAbuse) && Mode == ReCaptchaMode.PreventAbuse;
            var alwaysShow = Mode == ReCaptchaMode.AlwaysShow;
            var isConfigured = _settings != null;

            if (isConfigured && (abuseDetected || alwaysShow))
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

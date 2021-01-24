using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.ReCaptcha.ActionFilters;
using OrchardCore.ReCaptcha.ActionFilters.Detection;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.ReCaptcha.TagHelpers;
using OrchardCore.ResourceManagement;

namespace OrchardCore.ReCaptcha
{
    internal static class ReCaptchaRenderer
    {
        internal static async Task ShowCaptchaOrCallback(
            ReCaptchaSettings settings,
            ReCaptchaMode mode,
            string Language,
            IEnumerable<IDetectRobots> robotDetectors,
            ILocalizationService localizationService,
            IResourceManager resourceManager,
            IStringLocalizer<ReCaptchaTagHelper> S,
            ILogger logger,
            Action<TagBuilder> useCaptchaDiv,
            Action otherwise)
        {
            var robotDetected = robotDetectors.Invoke(d => d.DetectRobot(), logger).Any(d => d.IsRobot) && mode == ReCaptchaMode.PreventRobots;
            var alwaysShow = mode == ReCaptchaMode.AlwaysShow;
            var isConfigured = settings != null;

            if (isConfigured && (robotDetected || alwaysShow))
            {
                var divBuilder = BuildCaptchaDiv(settings);
                useCaptchaDiv(divBuilder);

                var cultureInfo = await localizationService.GetCultureAsync(Language, logger, S);
                resourceManager.RegisterReCaptchaScript(settings, cultureInfo);
            }
            else
            {
                otherwise();
            }
        }

        internal static TagBuilder BuildCaptchaDiv(ReCaptchaSettings settings)
        {
            var builder = new TagBuilder("div");
            builder.Attributes.Add("class", "g-recaptcha");
            builder.Attributes.Add("data-sitekey", settings.SiteKey);

            return builder;
        }

        internal static void RegisterReCaptchaScript(this IResourceManager resourceManager, ReCaptchaSettings settings, CultureInfo cultureInfo)
        {
            var builder = new TagBuilder("script");
            var settingsUrl = $"{settings.ReCaptchaScriptUri}?hl={cultureInfo.TwoLetterISOLanguageName}";
            builder.Attributes.Add("src", settingsUrl);
            resourceManager.RegisterFootScript(builder);
        }

        private static async Task<CultureInfo> GetCultureAsync(this ILocalizationService localizationService, string language, ILogger logger, IStringLocalizer<ReCaptchaTagHelper> localizer)
        {
            CultureInfo culture = null;

            if (string.IsNullOrWhiteSpace(language))
                language = await localizationService.GetDefaultCultureAsync();

            try
            {
                culture = CultureInfo.GetCultureInfo(language);
            }
            catch (CultureNotFoundException)
            {
                logger.LogWarning(localizer["Language with name {0} not found", language]);
            }

            return culture;
        }
    }
}

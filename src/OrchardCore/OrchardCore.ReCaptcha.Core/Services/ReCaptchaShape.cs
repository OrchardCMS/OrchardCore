using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.ReCaptcha.Services;

[Feature("OrchardCore.ReCaptcha")]
public sealed class ReCaptchaShape : IShapeAttributeProvider
{
    private readonly ISiteService _siteService;
    private readonly ILocalizationService _localizationService;
    private readonly IResourceManager _resourceManager;
    private readonly ILogger _logger;

    public ReCaptchaShape(
        ISiteService siteService,
        ILocalizationService localizationService,
        IResourceManager resourceManager,
        ILogger<ReCaptchaShape> logger)
    {
        _siteService = siteService;
        _localizationService = localizationService;
        _resourceManager = resourceManager;
        _logger = logger;
    }

    [Shape]
    public async Task<IHtmlContent> ReCaptcha(string language, string onload)
    {
        var settings = await _siteService.GetSettingsAsync<ReCaptchaSettings>();

        if (!settings.ConfigurationExists())
        {
            return HtmlString.Empty;
        }

        var script = new TagBuilder("script");
        script.MergeAttribute("src", await GetReCaptchaScriptUrlAsync(settings.ReCaptchaScriptUri, language, onload));

        _resourceManager.RegisterFootScript(script);

        var div = new TagBuilder("div");
        div.AddCssClass("g-recaptcha");
        div.MergeAttribute("data-sitekey", settings.SiteKey);

        return div;
    }

    private async Task<string> GetReCaptchaScriptUrlAsync(string reCaptchaScriptUri, string language, string onload)
    {
        var query = new QueryString();
        var cultureInfo = await GetCultureAsync(language);
        if (cultureInfo != null)
        {
            query = query.Add("hl", cultureInfo.TwoLetterISOLanguageName);
        }

        if (!string.IsNullOrWhiteSpace(onload))
        {
            query = query.Add("onload", onload);
        }

        var settingsUrl = new UriBuilder(reCaptchaScriptUri)
        {
            Query = query.ToString(),
        };

        return settingsUrl.ToString();
    }

    private async Task<CultureInfo> GetCultureAsync(string language)
    {
        CultureInfo culture = null;

        if (string.IsNullOrWhiteSpace(language))
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

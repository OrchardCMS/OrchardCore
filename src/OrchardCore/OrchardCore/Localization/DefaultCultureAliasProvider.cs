using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Options;

namespace OrchardCore.Localization;

public class DefaultCultureAliasProvider : ICultureAliasProvider
{
    private readonly Dictionary<string, CultureInfo> _culturesAliases = new();

    private readonly OrchardCoreRequestLocalizationOptions _requestLocalizationOptions;

    public DefaultCultureAliasProvider(IOptions<OrchardCoreRequestLocalizationOptions> requestLocalizationOptions)
    {
        _requestLocalizationOptions = requestLocalizationOptions.Value;

        _culturesAliases.Add("zh-CN", new CultureInfo("zh-Hans-CN", _requestLocalizationOptions.UseUserOverride));
        _culturesAliases.Add("zh-TW", new CultureInfo("zh-Hant-TW", _requestLocalizationOptions.UseUserOverride));
    }

    public bool TryGetCulture(CultureInfo cultureAlias, out CultureInfo culture)
        => _culturesAliases.TryGetValue(cultureAlias.Name, out culture);
}

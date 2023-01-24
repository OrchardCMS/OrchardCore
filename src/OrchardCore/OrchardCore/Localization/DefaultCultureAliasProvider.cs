using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Options;

namespace OrchardCore.Localization;

public class DefaultCultureAliasProvider : ICultureAliasProvider
{
    private readonly Dictionary<string, CultureInfo> _culturesAliases = new();

    public DefaultCultureAliasProvider(IOptions<CultureOptions> cultureOptions)
    {
        var useUserOverride = !cultureOptions.Value.IgnoreSystemSettings;

        _culturesAliases.Add("zh-CN", new CultureInfo("zh-Hans-CN", useUserOverride));
        _culturesAliases.Add("zh-TW", new CultureInfo("zh-Hant-TW", useUserOverride));
    }

    public bool TryGetCulture(CultureInfo cultureAlias, out CultureInfo culture)
        => _culturesAliases.TryGetValue(cultureAlias.Name, out culture);
}

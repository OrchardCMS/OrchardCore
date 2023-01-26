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
        _culturesAliases.Add("zh-Hans-CN", new CultureInfo("zh-CN", useUserOverride));
        _culturesAliases.Add("zh-Hant-TW", new CultureInfo("zh-TW", useUserOverride));
    }

    public bool TryGetCulture(string culture, out CultureInfo cultureAlias)
        => _culturesAliases.TryGetValue(culture, out cultureAlias);
}

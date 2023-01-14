using System.Collections.Generic;
using System.Globalization;

namespace OrchardCore.Localization;

public class DefaultCultureAliasProvider : ICultureAliasProvider
{
    private static readonly Dictionary<string, CultureInfo> _culturesAliases = new()
    {
        { "zh-CN",  CultureInfo.GetCultureInfo("zh-Hans-CN")},
        { "zh-TW",  CultureInfo.GetCultureInfo("zh-Hant-TW")}
    };

    public bool TryGetCulture(CultureInfo cultureAlias, out CultureInfo culture)
    {
        if (_culturesAliases.TryGetValue(cultureAlias.Name, out culture))
        {
            return true;
        }

        return false;
    }
}

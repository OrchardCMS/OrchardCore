using System.Globalization;

namespace OrchardCore.Localization;

public sealed class DefaultCultureAliasProvider : ICultureAliasProvider
{
    private static readonly CultureInfo[] _cultureAliases =
    [
        CultureInfo.GetCultureInfo("zh-CN"),
        CultureInfo.GetCultureInfo("zh-TW")
    ];

    public IEnumerable<CultureInfo> GetCultureAliases() => _cultureAliases;
}

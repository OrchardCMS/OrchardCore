using System.Globalization;
using System.Linq;

namespace OrchardCore.Localization;

/// <summary>
/// Provides <see cref="CultureInfo"/> helper methods.
/// </summary>
public static class CultureInfoHelper
{

    private static readonly CultureInfo[] _cultureAliases = new[]
    {
            CultureInfo.GetCultureInfo("zh-CN"),
            CultureInfo.GetCultureInfo("zh-TW")
        };

    /// <summary>
    /// Gets all cultures including culture aliases.
    /// Add 'zh-CN' and 'zh-TW' culture aliases for backward compatibility.
    /// For more info: https://github.com/OrchardCMS/OrchardCore/issues/11672.
    /// </summary>
    public static CultureInfo[] GetAllCulturesWithAliases() =>
        CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Union(_cultureAliases)
            .ToArray();
}

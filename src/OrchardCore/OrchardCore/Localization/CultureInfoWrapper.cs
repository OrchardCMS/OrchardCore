using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Localization;

public static class CultureInfoWrapper
{
    /// <summary>
    /// Gets all cultures recognized by .NET, including culture aliases.
    /// </summary>
    public static CultureInfo[] GetCultures()
    {
        var cultureAliasProviders = ShellScope.Current.ServiceProvider.GetServices<ICultureAliasProvider>();

        var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).AsEnumerable();

        if (cultureAliasProviders.Any())
        {
            cultures = cultures.Union(cultureAliasProviders.SelectMany(provider => provider.GetCultureAliases()));
        }

        return cultures
            .OrderBy(c => c.Name)
            .ToArray();
    }
}

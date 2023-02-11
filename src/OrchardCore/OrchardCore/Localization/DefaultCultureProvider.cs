using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OrchardCore.Localization;

/// <summary>
/// Represents a default implementation for providing .NET cultures, including culture aliases.
/// </summary>
public class DefaultCultureProvider : ICultureProvider
{
    private readonly IEnumerable<ICultureAliasProvider> _cultureAliasProviders;

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultCultureProvider"/> with culture alias providers.
    /// </summary>
    /// <param name="cultureAliasProviders">The <see cref="IEnumerable{ICultureAliasProvider}"/>.</param>
    public DefaultCultureProvider(IEnumerable<ICultureAliasProvider> cultureAliasProviders)
    {
        _cultureAliasProviders = cultureAliasProviders;
    }

    /// <inheritdoc/>
    public CultureInfo[] GetAllCulturesAndAliases()
    {
        var cultureAliases = _cultureAliasProviders
            .SelectMany(p => p.GetAliases())
            .Distinct()
            .Select(c => CultureInfo.GetCultureInfo(c));

        var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Union(cultureAliases)
                .OrderBy(c => c.Name);

        return cultures.ToArray();
    }
}

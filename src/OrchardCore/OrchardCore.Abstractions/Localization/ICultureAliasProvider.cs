using System.Globalization;

namespace OrchardCore.Localization;

public interface ICultureAliasProvider
{
    bool TryGetCulture(string cultureAlias, out CultureInfo culture);
}

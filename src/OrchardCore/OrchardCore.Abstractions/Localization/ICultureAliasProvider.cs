using System.Globalization;

namespace OrchardCore.Localization;

public interface ICultureAliasProvider
{
    bool TryGetCulture(CultureInfo cultureAlias, out CultureInfo culture);
}

using System.Globalization;

namespace OrchardCore.Localization;

public interface ICultureAliasProvider
{
    bool TryGetCulture(string culture, out CultureInfo cultureAlias);
}

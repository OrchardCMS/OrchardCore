using System.Globalization;

namespace OrchardCore.Localization;

public interface ICultureAliasProvider
{
    IEnumerable<CultureInfo> GetCultureAliases();
}

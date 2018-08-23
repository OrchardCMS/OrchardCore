using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization
{
    public interface IPluralizeStringLocalizer : IStringLocalizer
    {
        LocalizedString Pluralize(string name, int count);

        PluralizationRule GetPluralRule(string twoLetterISOLanguageName);
    }
}

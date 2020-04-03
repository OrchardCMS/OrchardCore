using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization
{
    public interface IPluralStringLocalizer : IStringLocalizer
    {
        (LocalizedString, object[]) GetTranslation(string name, params object[] arguments);
    }
}

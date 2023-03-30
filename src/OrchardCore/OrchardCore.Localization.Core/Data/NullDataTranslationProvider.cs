using OrchardCore.Localization.Data;

namespace OrchardCore.Localization.Data;

internal class NullDataTranslationProvider : IDataTranslationProvider
{
    public void LoadTranslations(string cultureName, CultureDictionary dictionary)
    {

    }
}

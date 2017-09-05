namespace OrchardCore.Localization
{
    public interface ITranslationProvider
    {
        void LoadTranslations(string cultureName, CultureDictionary dictionary);
    }
}

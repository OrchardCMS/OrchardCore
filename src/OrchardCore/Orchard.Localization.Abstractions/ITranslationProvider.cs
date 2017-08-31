namespace Orchard.Localization
{
    public interface ITranslationProvider
    {
        void LoadTranslations(string cultureName, CultureDictionary dictionary);
    }
}

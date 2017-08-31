namespace Orchard.Localization.Abstractions
{
    public interface ITranslationProvider
    {
        void LoadTranslations(string cultureName, CultureDictionary dictionary);
    }
}

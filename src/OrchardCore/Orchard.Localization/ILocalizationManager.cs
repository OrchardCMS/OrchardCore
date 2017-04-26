namespace Orchard.Localization
{
    public interface ILocalizationManager
    {
        CultureDictionary GetDictionary(string cultureName);
    }
}
using System.IO;

namespace Orchard.Localization.PortableObject
{
    public class PoFilesTranslationsProvider : ITranslationProvider
    {
        private readonly ILocalizationFileLocationProvider _poFilesLocationProvider;
        private readonly PoParser _parser;

        public PoFilesTranslationsProvider(ILocalizationFileLocationProvider poFileLocationProvider)
        {
            _poFilesLocationProvider = poFileLocationProvider;
            _parser = new PoParser();
        }

        public void LoadTranslations(string cultureName, CultureDictionary dictionary)
        {
            foreach (var location in _poFilesLocationProvider.GetLocations(cultureName))
            {
                LoadFileToDictionary(location, dictionary);
            }
        }

        private void LoadFileToDictionary(string path, CultureDictionary dictionary)
        {
            if (File.Exists(path))
            {
                using (var stream = File.OpenRead(path))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        dictionary.MergeTranslations(_parser.Parse(reader));
                    }
                }
            }
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OrchardCore.Localization.PortableObject
{
    public class PoFilesTranslationsProvider : ITranslationProvider
    {
        private readonly IEnumerable<ILocalizationFileLocationProvider> _poFilesLocationProviders;
        private readonly PoParser _parser;

        public PoFilesTranslationsProvider(IEnumerable<ILocalizationFileLocationProvider> poFileLocationProviders)
        {
            _poFilesLocationProviders = poFileLocationProviders;
            _parser = new PoParser();
        }

        public void LoadTranslations(string cultureName, CultureDictionary dictionary)
        {
            foreach (var provider in _poFilesLocationProviders.OrderBy(p => p.Order))
            {
                foreach (var location in provider.GetLocations(cultureName))
                {
                    LoadFileToDictionary(location, dictionary);
                }
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

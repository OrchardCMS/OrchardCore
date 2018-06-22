using System.IO;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Localization.PortableObject
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
            foreach (var fileInfo in _poFilesLocationProvider.GetLocations(cultureName))
            {
                LoadFileToDictionary(fileInfo, dictionary);
            }
        }

        private void LoadFileToDictionary(IFileInfo fileInfo, CultureDictionary dictionary)
        {
            if (fileInfo.Exists)
            {
                using (var stream = fileInfo.CreateReadStream())
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Orchard.Parser;
using System.IO;

namespace Orchard.Environment.Extensions.Manifests
{
    public class ManifestProvider : IManifestProvider
    {
        private const string ManifestFile = "module.txt";

        private IFileProvider _fileProvider;

        public ManifestProvider(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public int Priority { get { return 0; } }

        public IConfigurationBuilder GetManifestConfiguration(IConfigurationBuilder configurationBuilder, string subPath)
        {
            var manifestFileInfo = _fileProvider.GetFileInfo(Path.Combine(subPath, ManifestFile));

            if (!manifestFileInfo.Exists)
            {
                return configurationBuilder;
            }

            return
                configurationBuilder
                    .AddYamlFile(_fileProvider, Path.Combine(subPath, ManifestFile), true, false);
        }
    }
}

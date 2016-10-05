using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Orchard.Environment.Extensions.Info.Manifests;
using Orchard.Parser;
using System;
using System.Collections.Generic;
using System.IO;

namespace Orchard.Environment.Extensions.Info
{
    public class ManifestProvider : IManifestProvider
    {
        private const string ManifestFile = "module.txt";

        private IFileProvider _fileProvider;

        public ManifestProvider(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public IManifestInfo GetManifest(string subPath)
        {
            var manifestFileInfo = _fileProvider.GetFileInfo(subPath);

            if (!manifestFileInfo.Exists)
            {
                return new NotFoundManifestFile(manifestFileInfo);
            }

            var configurationContainer =
                new ConfigurationBuilder()
                    .SetBasePath(manifestFileInfo.PhysicalPath)
                    .AddYamlFile(ManifestFile, true);

            var config = configurationContainer.Build();

            return new ManifestInfo(
                manifestFileInfo,
                config);
        }
    }
}

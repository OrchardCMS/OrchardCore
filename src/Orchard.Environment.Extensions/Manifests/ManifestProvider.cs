using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Orchard.Parser;
using System;
using System.IO;

namespace Orchard.Environment.Extensions.Manifests
{
    public class ManifestProvider : IManifestProvider
    {
        private IFileProvider _fileProvider;

        public ManifestProvider(IHostingEnvironment hostingEnvironment)
        {
            _fileProvider = hostingEnvironment.ContentRootFileProvider;
        }

        public int Priority { get { return 0; } }

        public IConfigurationBuilder GetManifestConfiguration(
            IConfigurationBuilder configurationBuilder, 
            string subPath)
        {
            // TODO.. (ngm) are there any better checks for IsYaml
            var extension = Path.GetExtension(subPath);

            if (!extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
            {
                return configurationBuilder;
            }

            var manifestFileInfo = _fileProvider.GetFileInfo(subPath);

            if (!manifestFileInfo.Exists)
            {
                return configurationBuilder;
            }

            return
                configurationBuilder
                    .AddYamlFile(_fileProvider, subPath, true, false);
        }
    }
}

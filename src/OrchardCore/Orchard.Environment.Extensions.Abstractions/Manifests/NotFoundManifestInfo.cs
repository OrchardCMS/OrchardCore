using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Orchard.Environment.Extensions.Manifests
{
    public class NotFoundManifestInfo : IManifestInfo
    {
        private readonly IFileInfo _fileInfo;

        public NotFoundManifestInfo(string subPath)
        {
            _fileInfo = new NotFoundFileInfo(subPath);
        }

        public IFileInfo Manifest { get { return _fileInfo; } }
        public bool Exists => false;
        public string Name => null;
        public string Description => null;
        public string Type => null;
        public string Author => null;
        public string Website => null;
        public Version Version => null;
        public IEnumerable<string> Tags => Enumerable.Empty<string>();
        public IConfigurationRoot ConfigurationRoot => null;
    }
}

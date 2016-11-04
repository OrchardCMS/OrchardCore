using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Configuration;

namespace Orchard.Environment.Extensions.Manifests
{
    public class NotFoundManifestInfo : IManifestInfo
    {
        private readonly string _subPath;
        private readonly IFileInfo _fileInfo;

        public NotFoundManifestInfo(string subPath)
        {
            _subPath = subPath;
            _fileInfo = new NotFoundFileInfo(subPath);
        }

        public IFileInfo Manifest { get { return _fileInfo; } }
        public bool Exists => false;
        public string Name { get; } = null;
        public string Description { get; } = null;
        public string Type { get; }
        public IConfigurationRoot ConfigurationRoot { get; }
    }
}

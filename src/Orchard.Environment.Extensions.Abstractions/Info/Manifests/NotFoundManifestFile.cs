using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Environment.Extensions.Info.Manifests
{
    public class NotFoundManifestFile : IManifestInfo
    {
        public NotFoundManifestFile(
            IFileInfo manifest
            )
        {
            Manifest = manifest;
        }

        public IFileInfo Manifest { get; private set; }
        public bool Exists => true;
        public string Name { get; } = null;
        public string Description { get; } = null;
        public IDictionary<string, string> Attributes { get; } = new Dictionary<string, string>();
    }
}

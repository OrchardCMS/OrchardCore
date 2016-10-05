using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;

namespace Orchard.Environment.Extensions.Info.Manifests
{
    public class ManifestInfo : IManifestInfo
    {
        public ManifestInfo(
            IFileInfo manifest,
            string name,
            string description,
            IDictionary<string, string> attributes
            )
        {
            Manifest = manifest;
            Name = name;
            Description = description;
            Attributes = attributes;
        }

        public IFileInfo Manifest { get; private set; }
        public bool Exists => true;
        public string Name { get; private set; }
        public string Description { get; private set; }
        public IDictionary<string, string> Attributes { get; private set; }
    }
}

using System;

namespace Orchard.Environment.Extensions.Info
{
    public class FeatureInfo : IFeatureInfo
    {
        public FeatureInfo(
            string id,
            string name,
            IExtensionInfo extension,
            string[] dependencies)
        {
            Id = id;
            Name = name;
            Extension = extension;
            Dependencies = dependencies;
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public IExtensionInfo Extension { get; private set; }
        public string[] Dependencies { get; private set; }

    }
}

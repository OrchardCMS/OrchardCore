using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Info
{
    public class FeatureInfo : IFeatureInfo
    {
        public FeatureInfo(
            string id,
            string name,
            IExtensionInfo extension,
            IEnumerable<IFeatureInfo> dependencies)
        {
            Id = id;
            Name = name;
            Extension = extension;
            Dependencies = dependencies;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public IExtensionInfo Extension { get; set; }
        public IEnumerable<IFeatureInfo> Dependencies { get; set; }
            = new HashSet<IFeatureInfo>();
    }
}

using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Info
{
    public interface IFeatureInfo
    {
        string Id { get; set; }
        string Name { get; set; }
        IExtensionInfo Extension { get; set; }
        IEnumerable<IFeatureInfo> Dependencies { get; set; }
    }
}

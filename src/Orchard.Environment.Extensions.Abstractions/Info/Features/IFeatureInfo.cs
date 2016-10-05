using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Info
{
    public interface IFeatureInfo
    {
        string Id { get; }
        string Name { get; }
        IExtensionInfo Extension { get; }
        IEnumerable<IFeatureInfo> Dependencies { get; }
    }
}

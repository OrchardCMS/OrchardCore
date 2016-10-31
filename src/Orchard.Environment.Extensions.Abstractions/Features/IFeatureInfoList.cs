using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Features
{
    public interface IFeatureInfoList : IReadOnlyList<IFeatureInfo>
    {
        IFeatureInfo this[string key] { get; }
        IEnumerable<IExtensionInfo> Extensions { get; }
    }
}

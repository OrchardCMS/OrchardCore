using Orchard.Environment.Extensions.Info.Features;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Info.Extensions
{
    public interface IExtensionInfoList : IReadOnlyList<IExtensionInfo>
    {
        IExtensionInfo this[string key] { get; }
        IEnumerable<IFeatureInfo> GetAllFeatures();
    }
}

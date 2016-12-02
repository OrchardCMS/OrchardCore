using Orchard.Environment.Extensions.Features;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions
{
    public interface IExtensionInfoList : IEnumerable<IExtensionInfo>
    {
        IExtensionInfo this[string key] { get; }
        IEnumerable<IFeatureInfo> Features { get; }
    }
}

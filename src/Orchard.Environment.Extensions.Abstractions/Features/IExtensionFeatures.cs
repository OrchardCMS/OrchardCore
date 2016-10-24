using System.Collections;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Features
{
    public interface IExtensionFeatures : IEnumerable<IFeatureInfo>, IEnumerable
    {
    }
}

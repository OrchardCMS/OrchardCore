using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell.Descriptor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Environment.Shell
{
    public interface IShellFeaturesManager
    {
        IEnumerable<IFeatureInfo> EnabledFeatures(ShellDescriptor shell);
    }
}

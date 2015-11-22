using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Shell.Descriptor.Models
{
    /// <summary>
    /// Contains a snapshot of a tenant's enabled features.
    /// The information is drawn out of the shell via IShellDescriptorManager
    /// and cached by the host via IShellDescriptorCache. It is
    /// passed to the ICompositionStrategy to build the ShellBlueprint.
    /// </summary>
    public class ShellDescriptor
    {
        public ShellDescriptor()
        {
            Features = new List<ShellFeature>();
            Parameters = new List<ShellParameter>();
        }

        public int SerialNumber { get; set; }
        public IList<ShellFeature> Features { get; set; }
        public IList<ShellParameter> Parameters { get; set; }
    }

    public class ShellFeature
    {
        public string Name { get; set; }
    }

    public class ShellParameter
    {
        public string Component { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
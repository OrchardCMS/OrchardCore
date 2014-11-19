using System.Collections.Generic;
using System.Linq;

namespace OrchardVNext.Environment.Descriptor.Models {
    /// <summary>
    /// Contains a snapshot of a tenant's enabled features.
    /// The information is drawn out of the shell via IShellDescriptorManager
    /// and cached by the host via IShellDescriptorCache. It is
    /// passed to the ICompositionStrategy to build the ShellBlueprint.
    /// </summary>
    public class ShellDescriptor {
        public ShellDescriptor() {
            Features = Enumerable.Empty<ShellFeature>();
            Parameters = Enumerable.Empty<ShellParameter>();
        }

        public int SerialNumber { get; set; }
        public IEnumerable<ShellFeature> Features { get; set; }
        public IEnumerable<ShellParameter> Parameters { get; set; }
    }

    public class ShellFeature {
        public string Name { get; set; }
    }

    public class ShellParameter {
        public string Component { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
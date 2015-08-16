using OrchardVNext.Environment.Descriptor.Models;
using OrchardVNext.Environment.Extensions.Models;
using System;
using System.Collections.Generic;
using OrchardVNext.Configuration.Environment;

namespace OrchardVNext.Environment.ShellBuilders.Models {
    /// <summary>
    /// Contains the information necessary to initialize an IoC container
    /// for a particular tenant. This model is created by the ICompositionStrategy
    /// and is passed into the IShellContainerFactory.
    /// </summary>
    public class ShellBlueprint {
        public ShellSettings Settings { get; set; }
        public ShellDescriptor Descriptor { get; set; }

        public IEnumerable<DependencyBlueprint> Dependencies { get; set; }
    }

    public class ShellBlueprintItem {
        public Type Type { get; set; }
        public Feature Feature { get; set; }
    }

    public class DependencyBlueprint : ShellBlueprintItem {
        public IEnumerable<ShellParameter> Parameters { get; set; }
    }
}
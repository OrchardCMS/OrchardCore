using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Modules.Models;

namespace Orchard.Modules.ViewModels {
    public class FeaturesViewModel {
        public IEnumerable<ModuleFeature> Features { get; set; }
        public FeaturesBulkAction BulkAction { get; set; }
        public Func<ExtensionDescriptor, bool> IsAllowed { get; set; }
    }

    public enum FeaturesBulkAction {
        None,
        Enable,
        Disable,
        Update,
        Toggle
    }
}
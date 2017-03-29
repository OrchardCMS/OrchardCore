using System;
using System.Collections.Generic;
using Orchard.Modules.Models;
using Orchard.Environment.Extensions;

namespace Orchard.Modules.ViewModels {
    public class FeaturesViewModel {
        public IEnumerable<ModuleFeature> Features { get; set; }
        public FeaturesBulkAction BulkAction { get; set; }
        public Func<IExtensionInfo, bool> IsAllowed { get; set; }
    }

    public enum FeaturesBulkAction {
        None,
        Enable,
        Disable,
        Update,
        Toggle
    }
}
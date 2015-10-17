using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Tests.Stubs {
    public class StubExtensionManager : IExtensionManager {
        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            throw new NotImplementedException();
        }

        public IEnumerable<FeatureDescriptor> AvailableFeatures() {
            throw new NotImplementedException();
        }

        public ExtensionDescriptor GetExtension(string id) {
            throw new NotImplementedException();
        }

        public bool HasDependency(FeatureDescriptor item, FeatureDescriptor subject) {
            throw new NotImplementedException();
        }

        public IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors) {
            throw new NotImplementedException();
        }
    }
}

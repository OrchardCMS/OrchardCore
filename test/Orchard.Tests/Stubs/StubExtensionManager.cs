using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using System.Linq;

namespace Orchard.Tests.Stubs
{
    public class StubExtensionManager : IExtensionManager
    {
        public IEnumerable<ExtensionDescriptor> AvailableExtensions()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FeatureDescriptor> AvailableFeatures()
        {
            throw new NotImplementedException();
        }

        public ExtensionDescriptor GetExtension(string id)
        {
            throw new NotImplementedException();
        }

        public bool HasDependency(FeatureDescriptor item, FeatureDescriptor subject)
        {
            if (DefaultExtensionTypes.IsTheme(item.Extension.ExtensionType))
            {
                if (DefaultExtensionTypes.IsModule(subject.Extension.ExtensionType))
                {
                    // Themes implicitly depend on modules to ensure build and override ordering
                    return true;
                }

                if (DefaultExtensionTypes.IsTheme(subject.Extension.ExtensionType))
                {
                    // Theme depends on another if it is its base theme
                    return item.Extension.BaseTheme == subject.Id;
                }
            }

            // Return based on explicit dependencies
            return item.Dependencies != null &&
                   item.Dependencies.Any(x => StringComparer.OrdinalIgnoreCase.Equals(x, subject.Id));
        }

        public IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;

namespace Orchard.DisplayManagement.Extensions
{
    public class ThemeExtensionOrderingStrategy : IExtensionOrderingStrategy
    {
        public bool HasDependency(IFeatureInfo observer, IFeatureInfo subject)
        {
            if (subject.Extension.Manifest.IsTheme()) {
                bool isDependent = !observer.Extension.Manifest.IsTheme();
                return isDependent;
            }
            return false;
        }
    }
}

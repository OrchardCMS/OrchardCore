using System;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;

namespace Orchard.DisplayManagement.Extensions
{
    public class ThemeExtensionOrderingStrategy : IExtensionOrderingStrategy
    {
        public bool HasDependency(IFeatureInfo observer, IFeatureInfo subject)
        {
            if (observer.Extension.Manifest.IsTheme()) {
                return !subject.Extension.Manifest.IsTheme();
            }
            return false;
        }
    }
}

using System;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;

namespace Orchard.DisplayManagement.Extensions
{
    public class ThemeExtensionOrderingStrategy : IExtensionOrderingStrategy
    {

        public double Priority => 10D;

        public int Compare(IFeatureInfo observer, IFeatureInfo subject)
        {
            if (observer.Extension.Manifest.IsTheme())
            {
                if (subject.Extension.Manifest.IsTheme())
                {
                    return -1;
                }
                return 1;
            }
            return 0;
        }
    }
}

using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;

namespace Orchard.DisplayManagement.Extensions
{
    public class ThemeExtensionOrderingStrategy : IExtensionOrderingStrategy
    {
        public double Priority => 10D;

        public int Compare(IFeatureInfo observer, IFeatureInfo subject)
        {
            var isObserverTheme = observer.Extension.Manifest.IsTheme();
            var isSubjectTheme = subject.Extension.Manifest.IsTheme();

            if (isObserverTheme && !isSubjectTheme)
            {
                return 1;
            }

            if (!isObserverTheme && isSubjectTheme)
            {
                return -1;
            }

            return 0;
        }
    }
}
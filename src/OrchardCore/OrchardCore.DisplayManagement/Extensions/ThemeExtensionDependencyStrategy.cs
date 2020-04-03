using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.DisplayManagement.Extensions
{
    public class ThemeExtensionDependencyStrategy : IExtensionDependencyStrategy
    {
        public bool HasDependency(IFeatureInfo observer, IFeatureInfo subject)
        {
            if (observer.Extension.IsTheme())
            {
                if (!subject.Extension.IsTheme())
                    return true;
            }

            return false;
        }
    }
}
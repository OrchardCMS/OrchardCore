using System.Linq;
using Orchard.Environment.Extensions.Manifests;

namespace Orchard.Environment.Extensions.Features
{
    public static class FeatureExtensions
    {
        public static bool DependencyOn(this IFeatureInfo observer, IFeatureInfo subject)
        {
            return observer.Dependencies.Any(x => x == subject.Id);
        }

        /// <summary>
        /// Returns true if the item has an explicit or implicit dependency on the subject
        /// </summary>
        public static bool HasDependency(this IFeatureInfo observer, IFeatureInfo subject)
        {
            if (observer.Extension.Manifest.IsTheme())
            {
                if (subject.Extension.Manifest.IsCore())
                {
                    return true;
                }

                if (subject.Extension.Manifest.IsModule())
                {
                    return true;
                }

                if (subject.Extension.Manifest.IsTheme())
                {
                    var theme = new ThemeExtensionInfo(observer.Extension);

                    if (theme.HasBaseTheme())
                    {
                        return theme.BaseTheme == subject.Id;
                    }
                }
            }

            return observer.DependencyOn(subject);
        }
    }
}

using System.Linq;

namespace Orchard.Environment.Extensions.Features
{
    public static class FeatureExtensions
    {
        public static bool ObserverHasADependencyOnSubject(this IExtensionManager extensionManager, IFeatureInfo observer, IFeatureInfo subject)
        {
            return extensionManager
                .GetDependentFeatures(observer.Id, extensionManager.GetExtensions().Features.ToArray())
                .Contains(subject);
        }
    }
}

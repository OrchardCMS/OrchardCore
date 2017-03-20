using System.Linq;
using OrchardCore.Extensions.Features;

namespace OrchardCore.Extensions
{
    public class ExtensionDependencyStrategy : IExtensionDependencyStrategy
    {
        public bool HasDependency(IFeatureInfo observer, IFeatureInfo subject)
        {
            return observer.Dependencies.Contains(subject.Id);
        }
    }
}
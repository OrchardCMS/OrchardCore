using System.Linq;
using Orchard.Environment.Extensions.Features;

namespace Orchard.Environment.Extensions
{
    public class ExtensionDependencyStrategy : IExtensionDependencyStrategy
    {
        public bool HasDependency(IFeatureInfo observer, IFeatureInfo subject)
        {
            return observer.Dependencies.Contains(subject.Id);
        }
    }
}
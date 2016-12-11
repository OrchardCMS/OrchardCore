using Orchard.Environment.Extensions.Features;

namespace Orchard.Environment.Extensions
{
    public interface IExtensionOrderingStrategy
    {
        bool HasDependency(IFeatureInfo observer, IFeatureInfo subject);
    }
}

using OrchardCore.Extensions.Features;

namespace OrchardCore.Extensions
{
    public interface IExtensionDependencyStrategy
    {
        bool HasDependency(IFeatureInfo observer, IFeatureInfo subject);
    }
}

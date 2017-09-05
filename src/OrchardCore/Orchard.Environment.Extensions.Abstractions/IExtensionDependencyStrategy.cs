using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions
{
    public interface IExtensionDependencyStrategy
    {
        bool HasDependency(IFeatureInfo observer, IFeatureInfo subject);
    }
}

using OrchardCore.Extensions.Features;

namespace OrchardCore.Extensions
{
    public interface IExtensionPriorityStrategy
    {
        int GetPriority(IFeatureInfo feature);
    }
}

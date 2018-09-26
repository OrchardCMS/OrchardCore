using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions
{
    public interface IExtensionPriorityStrategy
    {
        int GetPriority(IFeatureInfo feature);
    }
}

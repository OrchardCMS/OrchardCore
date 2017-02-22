using Orchard.Environment.Extensions.Features;

namespace Orchard.Environment.Extensions
{
    public interface IExtensionPriorityStrategy
    {
        int GetPriority(IFeatureInfo feature);
    }
}

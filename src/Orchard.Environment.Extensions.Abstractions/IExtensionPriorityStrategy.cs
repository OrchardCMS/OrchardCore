using Orchard.Environment.Extensions.Features;

namespace Orchard.Environment.Extensions
{
    public interface IExtensionPriorityStrategy
    {
        double GetPriority(IFeatureInfo feature);
    }
}

using Orchard.Environment.Extensions.Features;

namespace Orchard.Environment.Extensions
{
    public class ExtensionPriorityStrategy : IExtensionPriorityStrategy
    {
        public double GetPriority(IFeatureInfo feature)
        {
            return feature.Priority;
        }
    }
}
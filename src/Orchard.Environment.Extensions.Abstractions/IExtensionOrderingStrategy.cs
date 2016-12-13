using Orchard.Environment.Extensions.Features;

namespace Orchard.Environment.Extensions
{
    public interface IExtensionOrderingStrategy
    {
        double Priority { get; }
        int Compare(IFeatureInfo observer, IFeatureInfo subject);
    }
}

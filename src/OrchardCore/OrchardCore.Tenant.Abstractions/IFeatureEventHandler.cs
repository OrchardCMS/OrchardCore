using OrchardCore.Extensions.Features;
using Orchard.Events;

namespace OrchardCore.Tenant
{
    public interface IFeatureEventHandler : IEventHandler
    {
        void Installing(IFeatureInfo feature);
        void Installed(IFeatureInfo feature);
        void Enabling(IFeatureInfo feature);
        void Enabled(IFeatureInfo feature);
        void Disabling(IFeatureInfo feature);
        void Disabled(IFeatureInfo feature);
        void Uninstalling(IFeatureInfo feature);
        void Uninstalled(IFeatureInfo feature);
    }
}

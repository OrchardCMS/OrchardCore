using System.Threading.Tasks;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Shell
{
    public interface IFeatureEventHandler
    {
        Task EnablingAsync(IFeatureInfo feature);
        Task EnabledAsync(IFeatureInfo feature);
        Task DisablingAsync(IFeatureInfo feature);
        Task DisabledAsync(IFeatureInfo feature);
    }
}

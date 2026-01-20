using System.Threading.Tasks;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Shell;

public class FeatureEventHandler : IFeatureEventHandler
{
    public virtual Task DisabledAsync(IFeatureInfo feature) => Task.CompletedTask;

    public virtual Task DisablingAsync(IFeatureInfo feature) => Task.CompletedTask;

    public virtual Task EnabledAsync(IFeatureInfo feature) => Task.CompletedTask;

    public virtual Task EnablingAsync(IFeatureInfo feature) => Task.CompletedTask;

    public virtual Task InstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

    public virtual Task InstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

    public virtual Task UninstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

    public virtual Task UninstallingAsync(IFeatureInfo feature) => Task.CompletedTask;
}

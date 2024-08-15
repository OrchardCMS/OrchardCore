using Microsoft.Extensions.DependencyInjection;
using OrchardCore.MetaWeblog;
using OrchardCore.Modules;

namespace OrchardCore.Autoroute.RemotePublishing;

[RequireFeatures("OrchardCore.RemotePublishing")]
public sealed class RemotePublishingStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IMetaWeblogDriver, AutorouteMetaWeblogDriver>();
    }
}

using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.MetaWeblog;
using OrchardCore.Extensions.Features.Attributes;

namespace Orchard.Autoroute.RemotePublishing
{

    [OrchardFeature("Orchard.RemotePublishing")]
    public class RemotePublishingStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IMetaWeblogDriver, AutorouteMetaWeblogDriver>();
        }
    }
}

using OrchardCore.Extensions.Features.Attributes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using Orchard.MetaWeblog;

namespace Orchard.Body.RemotePublishing
{

    [OrchardFeature("Orchard.RemotePublishing")]
    public class RemotePublishingStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IMetaWeblogDriver, BodyMetaWeblogDriver>();
        }
    }
}

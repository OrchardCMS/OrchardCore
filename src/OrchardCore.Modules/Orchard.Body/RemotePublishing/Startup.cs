using Orchard.Environment.Extensions.Features.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Modules;
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

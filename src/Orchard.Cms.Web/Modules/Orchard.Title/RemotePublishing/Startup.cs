using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Extensions.Features.Attributes;
using Orchard.MetaWeblog;

namespace Orchard.Title.RemotePublishing
{

    [OrchardFeature("Orchard.RemotePublishing")]
    public class RemotePublishingStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IMetaWeblogDriver, TitleMetaWeblogDriver>();
        }
    }
}

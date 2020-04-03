using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.MetaWeblog;

namespace OrchardCore.Title.RemotePublishing
{

    [RequireFeatures("OrchardCore.RemotePublishing")]
    public class RemotePublishingStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IMetaWeblogDriver, TitleMetaWeblogDriver>();
        }
    }
}

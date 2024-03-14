using Microsoft.Extensions.DependencyInjection;
using OrchardCore.MetaWeblog;
using OrchardCore.Modules;

namespace OrchardCore.Html.RemotePublishing
{
    [RequireFeatures("OrchardCore.RemotePublishing")]
    public class RemotePublishingStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IMetaWeblogDriver, HtmlBodyMetaWeblogDriver>();
        }
    }
}

using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.MetaWeblog;

namespace OrchardCore.Markdown.RemotePublishing
{

    [RequireFeatures("OrchardCore.RemotePublishing")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IMetaWeblogDriver, MarkdownBodyMetaWeblogDriver>();
        }
    }
}

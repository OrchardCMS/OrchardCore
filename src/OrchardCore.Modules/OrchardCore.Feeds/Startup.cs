using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Feeds;
using OrchardCore.Modules;

namespace OrchardCore.Scripting
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddFeeds();
        }
    }
}

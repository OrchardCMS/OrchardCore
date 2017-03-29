using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Extensions.Features.Attributes;
using Orchard.MetaWeblog;

namespace Orchard.Markdown.RemotePublishing
{

    [OrchardFeature("Orchard.RemotePublishing")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IMetaWeblogDriver, MarkdownMetaWeblogDriver>();
        }
    }
}

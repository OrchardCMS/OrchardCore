using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

namespace OrchardCore.Shortcodes
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IShortcodeService, ShortcodeService>();
        }
    }
}

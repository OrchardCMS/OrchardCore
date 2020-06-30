using Microsoft.Extensions.DependencyInjection;
using Shortcodes;
using OrchardCore.Modules;
using OrchardCore.Shortcodes.Services;

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

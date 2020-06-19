using Microsoft.Extensions.DependencyInjection;
using Shortcodes;
using OrchardCore.Modules;
using OrchardCore.ShortCodes.Services;

namespace OrchardCore.ShortCodes
{
    public class Startup : StartupBase
    {
        // Register first so the shape short code provider is able to provide overrides for named shortcodes.
        public override int Order => -100;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IShortCodeService, ShortCodeService>();
            services.AddScoped<IShortcodeProvider, ShapeShortcodeProvider>();
        }
    }
}

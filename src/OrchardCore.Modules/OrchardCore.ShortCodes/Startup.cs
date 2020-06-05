using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.ShortCodes.Services;

namespace OrchardCore.ShortCodes
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IShortCodeService, ShortCodeService>();
        }
    }
}

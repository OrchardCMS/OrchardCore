using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Antiforgery
{
    public class Startup : StartupBase
    {
        private readonly string _tenantName;
        private readonly string _tenantPrefix;

        public Startup(ShellSettings shellSettings)
        {
            _tenantName = shellSettings.Name;
            _tenantPrefix = "/" + shellSettings.RequestUrlPrefix;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "orchantiforgery_" + _tenantName;
                options.Cookie.Path = _tenantPrefix;
            });
        }
    }
}

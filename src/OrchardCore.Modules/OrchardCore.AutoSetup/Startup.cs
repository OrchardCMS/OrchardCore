using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace OrchardCore.AutoSetup
{
    /// <summary>
    /// The AutoSetup feature startup.
    /// </summary>
    public sealed class Startup : StartupBase
    {
        /// <summary>
        /// The Shell/Tenant configuration.
        /// </summary>
        private readonly IShellConfiguration _shellConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="shellConfiguration">
        /// The shell configuration.
        /// </param>
        public Startup(IShellConfiguration shellConfiguration)
        {
            this._shellConfiguration = shellConfiguration;
        }

        /// <summary>
        /// The configure services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<Microsoft.AspNetCore.Hosting.IStartupFilter, AutoSetupStartupFilter>();
            var configuration = _shellConfiguration.GetSection("OrchardCore_AutoSetup");
            services.Configure<AutoSetupOptions>(configuration);
        }
    }
}

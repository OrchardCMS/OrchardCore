using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.AutoSetup.Options;
using OrchardCore.Environment.Shell;
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
        /// AutoSetup Configuration Section Name 
        /// </summary>
        private const string ConfigSectionName = "OrchardCore_AutoSetup";

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
            var configuration = _shellConfiguration.GetSection(ConfigSectionName);
            services.Configure<AutoSetupOptions>(configuration);
        }

        /// <summary>
        /// Configure application pipeline.
        /// </summary>
        /// <param name="app">
        /// The app.
        /// </param>
        /// <param name="routes">
        /// The routes.
        /// </param>
        /// <param name="serviceProvider">
        /// The "Shell Scope" service provider.
        /// </param>
        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var currentShellSettings = serviceProvider.GetRequiredService<ShellSettings>();
            var logger = serviceProvider.GetRequiredService<ILogger<Startup>>();


            if (currentShellSettings.State == Environment.Shell.Models.TenantState.Uninitialized)
            {
                var optionsAccessor = serviceProvider.GetRequiredService<IOptions<AutoSetupOptions>>();
                var options = optionsAccessor.Value;

                if (options != null)
                {
                    var validationContext = new ValidationContext(options, serviceProvider, null);
                    var validationErrors = options.Validate(validationContext);

                    if (validationErrors.Any())
                    {
                        var stringBuilder = new StringBuilder();
                        foreach (var error in validationErrors)
                        {
                            stringBuilder.Append(error.ErrorMessage);
                        }

                        logger.LogError("AutoSetup did not start, configuration has following errors: {errors}", stringBuilder.ToString());
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(options.AutoSetupPath))
                        {
                            app.UseMiddleware<AutoSetupMiddleware>();
                        }
                        else
                        {
                            app.Map(options.AutoSetupPath, appBuilder => appBuilder.UseMiddleware<AutoSetupMiddleware>());
                        }
                    }
                }
            }

            base.Configure(app, routes, serviceProvider);
        }
    }
}

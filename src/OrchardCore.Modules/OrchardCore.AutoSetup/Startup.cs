using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.AutoSetup.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Routing;

namespace OrchardCore.AutoSetup
{
    /// <summary>
    /// The AutoSetup feature startup.
    /// </summary>
    public sealed class Startup : StartupBase
    {
        /// <summary>
        /// AutoSetup Configuration Section Name.
        /// </summary>
        private const string ConfigSectionName = "OrchardCore_AutoSetup";

        /// <summary>
        /// The Shell settings.
        /// </summary>
        private readonly ShellSettings _shellSettings;

        /// <summary>
        /// The Shell/Tenant configuration.
        /// </summary>
        private readonly IShellConfiguration _shellConfiguration;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<Startup> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="shellSettings">The Shell settings.</param>
        /// <param name="shellConfiguration">The shell configuration.</param>
        /// <param name="logger">The logger.</param>
        public Startup(ShellSettings shellSettings, IShellConfiguration shellConfiguration, ILogger<Startup> logger)
        {
            _shellSettings = shellSettings;
            _shellConfiguration = shellConfiguration;
            _logger = logger;
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

            if (configuration.Exists())
            {
                services.Configure<AutoSetupOptions>(o => o.ConfigurationExists = true);
            }
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
            if (_shellSettings.IsUninitialized())
            {
                var options = serviceProvider.GetRequiredService<IOptions<AutoSetupOptions>>().Value;
                if (!options.ConfigurationExists)
                {
                    return;
                }

                var validationContext = new ValidationContext(options, serviceProvider, null);

                var validationErrors = options.Validate(validationContext);
                if (validationErrors.Any())
                {
                    var stringBuilder = new StringBuilder();
                    foreach (var error in validationErrors)
                    {
                        stringBuilder.Append(error.ErrorMessage + ' ');
                    }

                    _logger.LogError("AutoSetup did not start, configuration has following errors: {errors}", stringBuilder.ToString());
                }
                else if (String.IsNullOrWhiteSpace(options.AutoSetupPath))
                {
                    app.UseMiddleware<AutoSetupMiddleware>();
                }
                else
                {
                    app.MapWhen(ctx => ctx.Request.Path.StartsWithNormalizedSegments(options.AutoSetupPath),
                        appBuilder => appBuilder.UseMiddleware<AutoSetupMiddleware>());
                }
            }
        }
    }
}

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Email.Azure.Controllers;
using OrchardCore.Email.Azure.Services;
using OrchardCore.Email.Services;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Email.Azure
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;
        private readonly ILogger _logger;
        private readonly IShellConfiguration _configuration;

        public Startup(
            IOptions<AdminOptions> adminOptions,
            ILogger<Startup> logger,
            IShellConfiguration configuration)
        {
            _adminOptions = adminOptions.Value;
            _logger = logger;
            _configuration = configuration;
        }

        public override int Order => 10;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddTransient<IConfigureOptions<AzureEmailSettings>, AzureEmailSettingsConfiguration>();
            services.AddKeyedScoped<IEmailService, AzureEmailService>(nameof(AzureEmailService));

            var connectionString = _configuration[$"OrchardCore_Email_Azure:{nameof(AzureEmailSettings.ConnectionString)}"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogError("Azure Email is enabled but not active because the 'ConnectionString' is missing or empty in application configuration.");
            }
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "AzureEmail.Options",
                areaName: "OrchardCore.Email.Azure",
                pattern: _adminOptions.AdminUrlPrefix + "/AzureEmail/Options",
                defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.Options) }
            );
        }
    }
}

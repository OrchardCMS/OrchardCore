using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Email.Controllers;
using OrchardCore.Email.Drivers;
using OrchardCore.Email.Services;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Email
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddEmailServices()
                .AddScoped<IDisplayDriver<ISite>, EmailSettingsDisplayDriver>()
                .AddScoped<IPermissionProvider, Permissions>()
                .AddScoped<INavigationProvider, AdminMenu>();

            services.AddSmtpEmailProvider()
                .AddScoped<IDisplayDriver<ISite>, SmtpSettingsDisplayDriver>()
                .AddTransient<IConfigureOptions<SmtpSettings>, SmtpSettingsConfiguration>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "EmailTest",
                areaName: "OrchardCore.Email",
                pattern: _adminOptions.AdminUrlPrefix + "/Email/Test",
                defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.Test) }
            );
        }
    }
}

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
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IDisplayDriver<ISite>, EmailSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddTransient<IConfigureOptions<EmailSettings>, EmailSettingsConfiguration>();
        }
    }

    [RequireFeatures("OrchardCore.Email.Smtp")]
    public class SmtpStartup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public SmtpStartup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, SmtpPermissions>();
            services.AddScoped<IDisplayDriver<ISite>, SmtpSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, SmtpAdminMenu>();

            services.AddTransient<IConfigureOptions<SmtpEmailSettings>, SmtpEmailSettingsConfiguration>();
            services.AddScoped<IEmailService, SmtpEmailService>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "EmailIndex",
                areaName: "OrchardCore.Email",
                pattern: _adminOptions.AdminUrlPrefix + "/Email/Smtp/Index",
                defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.Index) }
            );
        }
    }
}

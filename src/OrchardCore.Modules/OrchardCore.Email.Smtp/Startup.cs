using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Email.Smtp.Controllers;
using OrchardCore.Email.Smtp.Drivers;
using OrchardCore.Email.Services;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Email.Smtp;

public class Startup : StartupBase
{
    private readonly AdminOptions _adminOptions;

    public Startup(IOptions<AdminOptions> adminOptions)
    {
        _adminOptions = adminOptions.Value;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IPermissionProvider, Permissions>();
        services.AddScoped<IDisplayDriver<ISite>, SmtpSettingsDisplayDriver>();
        services.AddScoped<INavigationProvider, AdminMenu>();
        services.AddScoped<SmtpEmailService>();
        services.AddScoped<IEmailService>(sp => sp.GetService<SmtpEmailService>());

        services.AddTransient<IConfigureOptions<SmtpEmailSettings>, SmtpEmailSettingsConfiguration>();
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "SmtpEmailIndex",
            areaName: "OrchardCore.Email.Smtp",
            pattern: _adminOptions.AdminUrlPrefix + "/Email/Smtp/Index",
            defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.Index) }
        );
    }
}

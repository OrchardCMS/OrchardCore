using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Notifications;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Sms.Activities;
using OrchardCore.Sms.Controllers;
using OrchardCore.Sms.Drivers;
using OrchardCore.Sms.Services;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Sms;

public class Startup : StartupBase
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly AdminOptions _adminOptions;

    public Startup(
        IHostEnvironment hostEnvironment,
        IOptions<AdminOptions> adminOptions)
    {
        _hostEnvironment = hostEnvironment;
        _adminOptions = adminOptions.Value;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSmsServices();
        services.AddPhoneFormatValidator();

        if (_hostEnvironment.IsDevelopment())
        {
            // Add Log provider.
            services.AddLogSmsProvider();
        }

        services.AddScoped<IPermissionProvider, SmsPermissionProvider>();
        services.AddScoped<INavigationProvider, AdminMenu>();
        services.AddScoped<IDisplayDriver<ISite>, SmsSettingsDisplayDriver>();
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "SMSTest",
            areaName: "OrchardCore.Sms",
            pattern: _adminOptions.AdminUrlPrefix + "/SMS/test",
            defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.Test) }
        );
    }
}

[Feature("OrchardCore.Sms.Twilio")]
public class TwilioStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient(TwilioSmsProvider.TechnicalName, client =>
        {
            client.BaseAddress = new Uri("https://api.twilio.com/2010-04-01/Accounts/");
        }).AddStandardResilienceHandler();

        services.AddTwilioSmsProvider()
            .AddScoped<IDisplayDriver<ISite>, TwilioSettingsDisplayDriver>();
    }
}

[Feature("OrchardCore.Notifications.Sms")]
public class NotificationsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<INotificationMethodProvider, SmsNotificationProvider>();
    }
}

[RequireFeatures("OrchardCore.Workflows")]
public class WorkflowsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<SmsTask, SmsTaskDisplayDriver>();
    }
}

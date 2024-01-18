using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Email.Drivers;
using OrchardCore.Email.Services;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Email;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IPermissionProvider, Permissions>();
        services.AddScoped<IDisplayDriver<ISite>, EmailSettingsDisplayDriver>();
        services.AddScoped<INavigationProvider, AdminMenu>();
        services.AddScoped<IEmailMessageValidator, EmailMessageValidator>();
        services.AddScoped<IEmailDeliveryService, NullEmailDeliveryService>();
        services.AddScoped<IEmailService, EmailService>();

        services.AddTransient<IConfigureOptions<EmailSettings>, EmailSettingsConfiguration>();
    }
}

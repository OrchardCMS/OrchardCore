using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Azure.Email.Drivers;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Email.Azure.Services;
using OrchardCore.Email.Services;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Email.Azure;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<AzureEmailOptions>, AzureEmailOptionsConfiguration>();

        services.AddEmailProviderOptionsConfiguration<AzureEmailProviderOptionsConfigurations>()
            .AddScoped<IDisplayDriver<ISite>, AzureEmailSettingsDisplayDriver>();
    }
}

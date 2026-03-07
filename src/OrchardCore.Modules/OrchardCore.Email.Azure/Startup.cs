using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Azure.Email.Drivers;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Email.Azure.Services;
using OrchardCore.Email.Services;
using OrchardCore.Modules;

namespace OrchardCore.Email.Azure;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<AzureEmailOptions>, AzureEmailOptionsConfiguration>();

        services.AddSiteDisplayDriver<AzureEmailSettingsDisplayDriver>();

        services.AddTransient<IEmailProvider, AzureEmailProvider>();
    }
}

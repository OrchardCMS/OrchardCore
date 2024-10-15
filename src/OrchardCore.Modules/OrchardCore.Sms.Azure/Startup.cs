using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Sms.Azure.Drivers;
using OrchardCore.Sms.Azure.Models;

namespace OrchardCore.Sms.Azure;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddAzureSmsProvider()
            .AddSiteDisplayDriver<AzureSettingsDisplayDriver>();

        services.Configure<DefaultAzureSmsOptions>(options =>
        {
            _shellConfiguration.GetSection("OrchardCore_Sms_AzureCommunicationServices").Bind(options);

            options.IsEnabled = options.ConfigurationExists();
        });
    }
}

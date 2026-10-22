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
            // The 'OrchardCore_Sms_AzureCommunicationServices' section is deprecated and will be removed in a future major version, use 'Sms:Azure' instead.
            _shellConfiguration.GetSectionCompat("Sms:Azure", "OrchardCore_Sms_AzureCommunicationServices").Bind(options);

            options.IsEnabled = options.ConfigurationExists();
        });
    }
}

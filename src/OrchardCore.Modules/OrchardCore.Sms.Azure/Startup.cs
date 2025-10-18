using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Azure.Core;
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

            if (options.Endpoint is null && !string.IsNullOrEmpty(options.ConnectionString))
            {
                var endpointString = ConnectionStringHelper.Extract(options.ConnectionString, "Endpoint");

                if (endpointString is not null && Uri.TryCreate(endpointString, UriKind.Absolute, out var endpointUri))
                {
                    options.Endpoint = endpointUri;
                }
            }

            options.IsEnabled = options.ConfigurationExists();
        });
    }
}

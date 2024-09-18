using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Azure.Email.Drivers;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Email.Azure.Services;
using OrchardCore.Email.Core;
using OrchardCore.Email.Services;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Email.Azure;

public sealed class Startup
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<AzureEmailOptions>, AzureEmailOptionsConfiguration>();

        services.AddEmailProviderOptionsConfiguration<AzureEmailProviderOptionsConfigurations>()
            .AddSiteDisplayDriver<AzureEmailSettingsDisplayDriver>();

        services.Configure<DefaultAzureEmailOptions>(options =>
        {
            _shellConfiguration.GetSection("OrchardCore_Email_AzureCommunicationServices").Bind(options);

            // The 'OrchardCore_Email_Azure' key can be removed in version 3. 
            _shellConfiguration.GetSection("OrchardCore_Email_Azure").Bind(options);

            options.IsEnabled = options.ConfigurationExists();
        });
    }
}

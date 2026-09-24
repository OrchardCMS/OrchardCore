using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Azure.Email.Drivers;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Email.Azure.Services;
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
        services.AddSignalOptionsChangeTokenSource<AzureEmailOptions>();

        services.AddTransient<IConfigureOptions<AzureEmailOptions>, AzureEmailOptionsConfiguration>();

        services.AddEmailProviderOptionsConfiguration<AzureEmailProviderOptionsConfigurations>()
            .AddSiteDisplayDriver<AzureEmailSettingsDisplayDriver>();

        services.Configure<DefaultAzureEmailOptions>(options =>
        {
            // The 'OrchardCore_Email_Azure' and 'OrchardCore_Email_AzureCommunicationServices' sections are deprecated and will be removed
            // in a future major version, use 'Email:Azure' instead.
            _shellConfiguration
                .GetSectionCompat("Email:Azure", "OrchardCore_Email_Azure", "OrchardCore_Email_AzureCommunicationServices")
                .Bind(options);

            options.IsEnabled = options.ConfigurationExists();
        });
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services;

public class AzureEmailSettingsConfiguration(IShellConfiguration shellConfiguration, ISiteService site) : IConfigureOptions<AzureEmailSettings>
{
    private readonly IShellConfiguration _shellConfiguration = shellConfiguration;
    private readonly ISiteService _site = site;

    public void Configure(AzureEmailSettings options)
    {
        var emailSettings = _site.GetSiteSettingsAsync()
            .GetAwaiter()
            .GetResult()
            .As<EmailSettings>();

        var section = _shellConfiguration.GetSection("OrchardCore_Email_Azure");

        options.DefaultSender = section.GetValue(nameof(options.DefaultSender), emailSettings.DefaultSender);
        options.ConnectionString = section.GetValue<string>(nameof(options.ConnectionString));
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services;

public class EmailSettingsConfiguration(IShellConfiguration shellConfiguration, ISiteService site) : IConfigureOptions<EmailSettings>
{
    private readonly IShellConfiguration _shellConfiguration = shellConfiguration;
    private readonly ISiteService _site = site;

    public void Configure(EmailSettings options)
    {
        var section = _shellConfiguration.GetSection("OrchardCore_Email");

        var emailSettings = _site.GetSiteSettingsAsync().GetAwaiter().GetResult().As<EmailSettings>();

        options.DefaultSender = section.GetValue(nameof(options.DefaultSender), emailSettings.DefaultSender);
    }
}

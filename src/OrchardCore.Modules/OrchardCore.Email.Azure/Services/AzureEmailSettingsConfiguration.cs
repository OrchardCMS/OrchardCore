using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Email.Services;

public class AzureEmailSettingsConfiguration : IConfigureOptions<AzureEmailSettings>
{
    private readonly IShellConfiguration _shellConfiguration;

    public AzureEmailSettingsConfiguration(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public void Configure(AzureEmailSettings options)
    {
        var section = _shellConfiguration.GetSection("OrchardCore_Email_Azure");

        section.Bind(options);
    }
}

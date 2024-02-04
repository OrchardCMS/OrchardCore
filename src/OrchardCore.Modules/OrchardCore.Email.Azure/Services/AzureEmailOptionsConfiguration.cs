using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Email.Services;

public class AzureEmailOptionsConfiguration : IConfigureOptions<AzureEmailOptions>
{
    private readonly IShellConfiguration _shellConfiguration;

    public AzureEmailOptionsConfiguration(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public void Configure(AzureEmailOptions options)
    {
        var section = _shellConfiguration.GetSection("OrchardCore_Email_Azure");

        section.Bind(options);
    }
}

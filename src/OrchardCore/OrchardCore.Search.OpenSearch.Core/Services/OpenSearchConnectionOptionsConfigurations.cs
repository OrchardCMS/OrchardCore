using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Search.OpenSearch.Core.Models;

namespace OrchardCore.Search.OpenSearch.Core.Services;

public sealed class OpenSearchConnectionOptionsConfigurations : IConfigureOptions<OpenSearchConnectionOptions>
{
    public const string ConfigSectionName = "OrchardCore_OpenSearch";

    private readonly IShellConfiguration _shellConfiguration;

    public OpenSearchConnectionOptionsConfigurations(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public void Configure(OpenSearchConnectionOptions options)
    {
        _shellConfiguration.GetSection(ConfigSectionName).Bind(options);

        if (options.Ports == null || options.Ports.Length == 0)
        {
            options.Ports = [9200];
        }

        if (!string.IsNullOrWhiteSpace(options.Url))
        {
            options.SetFileConfigurationExists(true);
        }
    }
}

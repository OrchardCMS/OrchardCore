using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public sealed class ElasticsearchConnectionOptionsConfigurations : IConfigureOptions<ElasticsearchConnectionOptions>
{
    public const string ConfigSectionName = "OrchardCore_Elasticsearch";

    private readonly IShellConfiguration _shellConfiguration;

    public ElasticsearchConnectionOptionsConfigurations(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public void Configure(ElasticsearchConnectionOptions options)
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

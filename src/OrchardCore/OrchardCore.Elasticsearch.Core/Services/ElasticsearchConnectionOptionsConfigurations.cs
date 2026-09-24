using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Elasticsearch.Core.Models;

namespace OrchardCore.Elasticsearch.Core.Services;

public sealed class ElasticsearchConnectionOptionsConfigurations : IConfigureOptions<ElasticsearchConnectionOptions>
{
    /// <summary>
    /// The name of the Elasticsearch configuration section.
    /// </summary>
    public const string SectionName = "Search:Elasticsearch";

    // The 'OrchardCore_Elasticsearch' section is deprecated and will be removed in a future major version, use 'Search:Elasticsearch' instead.
    public const string ConfigSectionName = "OrchardCore_Elasticsearch";

    private readonly IShellConfiguration _shellConfiguration;

    public ElasticsearchConnectionOptionsConfigurations(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public void Configure(ElasticsearchConnectionOptions options)
    {
        _shellConfiguration.GetSectionCompat(SectionName, ConfigSectionName).Bind(options);

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

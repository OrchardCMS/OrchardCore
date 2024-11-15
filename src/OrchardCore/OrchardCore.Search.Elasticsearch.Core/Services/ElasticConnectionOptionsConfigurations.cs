using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public sealed class ElasticConnectionOptionsConfigurations : IConfigureOptions<ElasticConnectionOptions>
{
    public const string ConfigSectionName = "OrchardCore_Elasticsearch";

    private readonly IShellConfiguration _shellConfiguration;
    private readonly ILogger _logger;

    public ElasticConnectionOptionsConfigurations(
        IShellConfiguration shellConfiguration,
        ILogger<ElasticConnectionOptionsConfigurations> logger)
    {
        _shellConfiguration = shellConfiguration;
        _logger = logger;
    }

    public void Configure(ElasticConnectionOptions options)
    {
        var fileOptions = _shellConfiguration.GetSection(ConfigSectionName).Get<ElasticConnectionOptions>()
             ?? new ElasticConnectionOptions();

        options.Url = fileOptions.Url;
        options.Ports = fileOptions.Ports;
        options.ConnectionType = fileOptions.ConnectionType;
        options.CloudId = fileOptions.CloudId;
        options.Username = fileOptions.Username;
        options.Password = fileOptions.Password;
        options.CertificateFingerprint = fileOptions.CertificateFingerprint;

        options.SetFileConfigurationExists(HasConnectionInfo(options));
    }

    private bool HasConnectionInfo(ElasticConnectionOptions elasticConnectionOptions)
    {
        if (elasticConnectionOptions == null)
        {
            _logger.LogError("Elasticsearch is enabled but not active because the configuration is missing.");
            return false;
        }

        var optionsAreValid = true;

        if (string.IsNullOrWhiteSpace(elasticConnectionOptions.Url))
        {
            _logger.LogError("Elasticsearch is enabled but not active because the 'Url' is missing or empty in application configuration.");
            optionsAreValid = false;
        }

        if (elasticConnectionOptions.Ports == null || elasticConnectionOptions.Ports.Length == 0)
        {
            _logger.LogError("Elasticsearch is enabled but not active because a port is missing in application configuration.");
            optionsAreValid = false;
        }

        return optionsAreValid;
    }
}

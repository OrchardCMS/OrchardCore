using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
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
        options.EnableApiVersioningHeader = fileOptions.EnableApiVersioningHeader;

        if (HasConnectionInfo(options))
        {
            options.SetFileConfigurationExists(true);
            options.SetConnectionSettings(GetConnectionSettings(options));
        }
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

    private static ConnectionSettings GetConnectionSettings(ElasticConnectionOptions elasticConfiguration)
    {
        var pool = GetConnectionPool(elasticConfiguration);

        var settings = new ConnectionSettings(pool);

        if (elasticConfiguration.ConnectionType != "CloudConnectionPool" && !string.IsNullOrWhiteSpace(elasticConfiguration.Username) && !string.IsNullOrWhiteSpace(elasticConfiguration.Password))
        {
            settings.BasicAuthentication(elasticConfiguration.Username, elasticConfiguration.Password);
        }

        if (!string.IsNullOrWhiteSpace(elasticConfiguration.CertificateFingerprint))
        {
            settings.CertificateFingerprint(elasticConfiguration.CertificateFingerprint);
        }

        if (elasticConfiguration.EnableApiVersioningHeader)
        {
            settings.EnableApiVersioningHeader();
        }

        return settings;
    }

    private static IConnectionPool GetConnectionPool(ElasticConnectionOptions elasticConfiguration)
    {
        var uris = elasticConfiguration.Ports.Select(port => new Uri($"{elasticConfiguration.Url}:{port}")).Distinct();
        IConnectionPool pool = null;
        switch (elasticConfiguration.ConnectionType)
        {
            case "SingleNodeConnectionPool":
                pool = new SingleNodeConnectionPool(uris.First());
                break;

            case "CloudConnectionPool":
                if (!string.IsNullOrWhiteSpace(elasticConfiguration.Username) && !string.IsNullOrWhiteSpace(elasticConfiguration.Password) && !string.IsNullOrWhiteSpace(elasticConfiguration.CloudId))
                {
                    var credentials = new BasicAuthenticationCredentials(elasticConfiguration.Username, elasticConfiguration.Password);
                    pool = new CloudConnectionPool(elasticConfiguration.CloudId, credentials);
                }
                break;

            case "StaticConnectionPool":
                pool = new StaticConnectionPool(uris);
                break;

            case "SniffingConnectionPool":
                pool = new SniffingConnectionPool(uris);
                break;

            case "StickyConnectionPool":
                pool = new StickyConnectionPool(uris);
                break;

            default:
                pool = new SingleNodeConnectionPool(uris.First());
                break;
        }

        return pool;
    }
}

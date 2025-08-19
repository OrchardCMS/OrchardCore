using System.Text;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Logging;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public sealed class ElasticsearchClientFactory : IElasticsearchClientFactory
{
    private readonly ILogger _logger;

    public ElasticsearchClientFactory(ILogger<ElasticsearchClientFactory> logger)
    {
        _logger = logger;
    }

    public ElasticsearchClient Create(ElasticsearchConnectionOptions configuration)
    {
        AuthorizationHeader authentication = configuration.AuthenticationType switch
        {
            ElasticsearchAuthenticationType.ApiKey => new ApiKey(configuration.ApiKey),
            ElasticsearchAuthenticationType.Base64ApiKey => new Base64ApiKey(configuration.Base64ApiKey),
            ElasticsearchAuthenticationType.KeyIdAndKey => new Base64ApiKey(Convert.ToBase64String(Encoding.UTF8.GetBytes($"{configuration.KeyId}:{configuration.Key}"))),
            _ => new BasicAuthentication(configuration.Username, configuration.Password),
        };

        var settings = configuration.ConnectionType switch
        {
            ElasticsearchConnectionType.CloudConnectionPool => new ElasticsearchClientSettings(configuration.CloudId, authentication),
            ElasticsearchConnectionType.StaticConnectionPool => new ElasticsearchClientSettings(new StaticNodePool(GetNodeUris(configuration)))
                .Authentication(authentication),
            ElasticsearchConnectionType.SniffingConnectionPool => new ElasticsearchClientSettings(new SniffingNodePool(GetNodeUris(configuration)))
                .Authentication(authentication),
            ElasticsearchConnectionType.StickyConnectionPool => new ElasticsearchClientSettings(new StickyNodePool(GetNodeUris(configuration)))
                .Authentication(authentication),
            _ => new ElasticsearchClientSettings(GetNodeUris(configuration).FirstOrDefault())
                .Authentication(authentication),
        };

        if (!string.IsNullOrWhiteSpace(configuration.CertificateFingerprint))
        {
            settings.CertificateFingerprint(configuration.CertificateFingerprint);
        }

        if (configuration.EnableDebugMode)
        {
            settings.EnableDebugMode(details =>
            {
                if (!_logger.IsEnabled(LogLevel.Debug))
                {
                    return;
                }

                if (details.RequestBodyInBytes != null)
                {
                    _logger.LogInformation("Elasticsearch request is: {Request}.", Encoding.UTF8.GetString(details.RequestBodyInBytes));
                }

                if (details.ResponseBodyInBytes != null)
                {
                    _logger.LogInformation("Elasticsearch response is: {Response}.", Encoding.UTF8.GetString(details.ResponseBodyInBytes));
                }
            });
        }

        if (configuration.EnableHttpCompression)
        {
            settings.EnableHttpCompression();
        }

        return new ElasticsearchClient(settings);
    }

    private static IEnumerable<Uri> GetNodeUris(ElasticsearchConnectionOptions elasticConfiguration)
    {
        if (string.IsNullOrEmpty(elasticConfiguration.Url))
        {
            return [];
        }

        return elasticConfiguration.Ports.Select(port => new Uri($"{elasticConfiguration.Url}:{port}")).Distinct();
    }
}

using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public sealed class ElasticsearchClientFactory : IElasticsearchClientFactory
{
    public ElasticsearchClient Create(ElasticsearchConnectionOptions configuration)
    {
        var basicAuthentication = new BasicAuthentication(configuration.Username, configuration.Password);

        var settings = configuration.ConnectionType switch
        {
            ElasticsearchConnectionType.CloudConnectionPool => new ElasticsearchClientSettings(configuration.CloudId, basicAuthentication),
            ElasticsearchConnectionType.StaticConnectionPool => new ElasticsearchClientSettings(new StaticNodePool(GetNodeUris(configuration)))
                .Authentication(basicAuthentication),
            ElasticsearchConnectionType.SniffingConnectionPool => new ElasticsearchClientSettings(new SniffingNodePool(GetNodeUris(configuration)))
                .Authentication(basicAuthentication),
            ElasticsearchConnectionType.StickyConnectionPool => new ElasticsearchClientSettings(new StickyNodePool(GetNodeUris(configuration)))
                .Authentication(basicAuthentication),
            _ => new ElasticsearchClientSettings(GetNodeUris(configuration).FirstOrDefault())
                .Authentication(basicAuthentication),
        };

        if (!string.IsNullOrWhiteSpace(configuration.CertificateFingerprint))
        {
            settings.CertificateFingerprint(configuration.CertificateFingerprint);
        }

        if (configuration.EnableDebugMode)
        {
            settings.EnableDebugMode();
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

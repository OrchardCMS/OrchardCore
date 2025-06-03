using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public class Elasticsearch1ClientFactory
{
    public static ElasticsearchClient Create(ElasticsearchConnectionOptions configuration)
    {
        var settings = configuration.ConnectionType switch
        {
            ElasticsearchConnectionType.CloudConnectionPool => new ElasticsearchClientSettings(configuration.CloudId, new BasicAuthentication(configuration.Username, configuration.Password)),
            ElasticsearchConnectionType.StaticConnectionPool => new ElasticsearchClientSettings(new StaticNodePool(GetNodeUris(configuration))),
            ElasticsearchConnectionType.SniffingConnectionPool => new ElasticsearchClientSettings(new SniffingNodePool(GetNodeUris(configuration))),
            ElasticsearchConnectionType.StickyConnectionPool => new ElasticsearchClientSettings(new StickyNodePool(GetNodeUris(configuration))),
            _ => new ElasticsearchClientSettings(GetNodeUris(configuration).FirstOrDefault()),
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

using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public class ElasticsearchClientFactory
{
    public static ElasticsearchClient Create(ElasticsearchConnectionOptions elasticConfiguration)
    {
        var settings = elasticConfiguration.ConnectionType switch
        {
            ElasticsearchConnectionType.CloudConnectionPool => new ElasticsearchClientSettings(elasticConfiguration.CloudId, new BasicAuthentication(elasticConfiguration.Username, elasticConfiguration.Password)),
            ElasticsearchConnectionType.StaticConnectionPool => new ElasticsearchClientSettings(new StaticNodePool(GetNodeUris(elasticConfiguration))),
            ElasticsearchConnectionType.SniffingConnectionPool => new ElasticsearchClientSettings(new SniffingNodePool(GetNodeUris(elasticConfiguration))),
            ElasticsearchConnectionType.StickyConnectionPool => new ElasticsearchClientSettings(new StickyNodePool(GetNodeUris(elasticConfiguration))),
            _ => new ElasticsearchClientSettings(GetNodeUris(elasticConfiguration).FirstOrDefault()),
        };

        if (!string.IsNullOrWhiteSpace(elasticConfiguration.CertificateFingerprint))
        {
            settings.CertificateFingerprint(elasticConfiguration.CertificateFingerprint);
        }

        settings.EnableHttpCompression();

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

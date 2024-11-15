using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public class ElasticSearchClientFactory
{
    public static ElasticsearchClient Create(ElasticConnectionOptions elasticConfiguration)
    {
        var settings = elasticConfiguration.ConnectionType switch
        {
            "CloudConnectionPool" => new ElasticsearchClientSettings(elasticConfiguration.CloudId, new BasicAuthentication(elasticConfiguration.Username, elasticConfiguration.Password)),
            "StaticConnectionPool" => new ElasticsearchClientSettings(new StaticNodePool(GetNodeUris(elasticConfiguration))),
            "SniffingConnectionPool" => new ElasticsearchClientSettings(new SniffingNodePool(GetNodeUris(elasticConfiguration))),
            "StickyConnectionPool" => new ElasticsearchClientSettings(new StickyNodePool(GetNodeUris(elasticConfiguration))),
            _ => new ElasticsearchClientSettings(GetNodeUris(elasticConfiguration).First()),
        };

        if (!string.IsNullOrWhiteSpace(elasticConfiguration.CertificateFingerprint))
        {
            settings.CertificateFingerprint(elasticConfiguration.CertificateFingerprint);
        }

        return new ElasticsearchClient(settings);
    }

    private static IEnumerable<Uri> GetNodeUris(ElasticConnectionOptions elasticConfiguration)
    {
        return elasticConfiguration.Ports.Select(port => new Uri($"{elasticConfiguration.Url}:{port}")).Distinct();
    }
}

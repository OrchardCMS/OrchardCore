using System.Text.Json.Serialization;
using Elastic.Clients.Elasticsearch.Mapping;
using OrchardCore.Search.Elasticsearch.Core.Json;

namespace OrchardCore.Search.Elasticsearch.Models;

public sealed class ElasticsearchIndexMap
{
    public string KeyFieldName { get; set; }

    [JsonConverter(typeof(DictionaryConverter))]
    public Dictionary<string, IProperty> Properties { get; init; } = [];

    [JsonConverter(typeof(DynamicTemplateListConverter))]
    public IList<IDictionary<string, DynamicTemplate>> DynamicTemplates { get; init; } = [];

    public SourceField SourceField { get; set; }
}

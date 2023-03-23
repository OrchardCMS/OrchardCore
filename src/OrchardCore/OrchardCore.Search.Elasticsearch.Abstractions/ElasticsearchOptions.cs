using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Elasticsearch;

public class ElasticsearchOptions
{
    public string IndexPrefix { get; set; }

    public Dictionary<string, JObject> Analyzers { get; } = new Dictionary<string, JObject>();
}

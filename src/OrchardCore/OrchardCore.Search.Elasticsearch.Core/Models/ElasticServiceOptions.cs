using Microsoft.Extensions.Options;

namespace OrchardCore.Search.Elasticsearch.Core.Models;

public class ElasticServiceOptions : IAsyncOptions
{
    public bool IsServiceVerified { get; set; }
}

using Nest;

namespace OrchardCore.Search.Elasticsearch.Core.Mappings;

internal sealed class ContainedPartModel
{
    [Keyword(Name = "Ids")]
    public string Ids { get; set; }

    [Number(Name = "Order")]
    public string Order { get; set; }
}

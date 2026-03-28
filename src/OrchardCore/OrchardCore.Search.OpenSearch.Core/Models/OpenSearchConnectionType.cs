namespace OrchardCore.Search.OpenSearch.Core.Models;

public enum OpenSearchConnectionType
{
    SingleNodeConnectionPool,
    StaticConnectionPool,
    SniffingConnectionPool,
    StickyConnectionPool,
}

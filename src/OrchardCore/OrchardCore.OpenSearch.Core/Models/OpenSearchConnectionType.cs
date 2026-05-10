namespace OrchardCore.OpenSearch.Core.Models;

public enum OpenSearchConnectionType
{
    SingleNodeConnectionPool,
    StaticConnectionPool,
    SniffingConnectionPool,
    StickyConnectionPool,
}

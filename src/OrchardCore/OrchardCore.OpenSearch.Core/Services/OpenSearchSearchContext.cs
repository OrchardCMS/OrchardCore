using OpenSearch.Client;
using OrchardCore.Indexing.Models;

namespace OrchardCore.OpenSearch.Core.Services;

public class OpenSearchSearchContext
{
    public IndexProfile IndexProfile { get; }

    public SearchRequest SearchRequest { get; }

    public OpenSearchSearchContext(IndexProfile indexProfile, SearchRequest searchRequest)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);
        ArgumentNullException.ThrowIfNull(searchRequest);

        IndexProfile = indexProfile;
        SearchRequest = searchRequest;
    }
}

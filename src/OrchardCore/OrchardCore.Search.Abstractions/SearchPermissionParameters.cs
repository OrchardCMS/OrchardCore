using System;

namespace OrchardCore.Search.Abstractions;

public class SearchPermissionParameters
{
    public SearchProvider Provider { get; }

    public string IndexName { get; }

    public SearchPermissionParameters(SearchProvider provider, string indexName)
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));

        IndexName = indexName;
    }
}

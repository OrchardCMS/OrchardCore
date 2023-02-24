using System;

namespace OrchardCore.Search.Abstractions;

public class SearchPermissionParameters
{
    public string ServiceName { get; }

    public string IndexName { get; }

    public SearchPermissionParameters(string serviceName, string indexName)
    {
        if (String.IsNullOrEmpty(serviceName))
        {
            throw new ArgumentException($"{nameof(serviceName)} cannot be null or empty");
        }

        ServiceName = serviceName;
        IndexName = indexName;
    }
}

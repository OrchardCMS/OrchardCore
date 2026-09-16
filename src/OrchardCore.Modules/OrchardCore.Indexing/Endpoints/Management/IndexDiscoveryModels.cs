using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Indexing.Endpoints.Management;

/// <summary>Filters and pages index definitions using the existing store paging contract.</summary>
public sealed class IndexListRequest
{
    /// <summary>Gets a name search using the configured store's comparison rules.</summary>
    public string Search { get; init; }

    /// <summary>Gets the one-based page number, defaulting to one.</summary>
    [Range(1, int.MaxValue)]
    public int? Page { get; init; }

    /// <summary>Gets the page size, defaulting to 50 and limited to 200.</summary>
    [Range(1, 200)]
    public int? PageSize { get; init; }
}

/// <summary>Describes the public identity of a stored index profile.</summary>
public sealed class IndexProfileResponse
{
    /// <summary>Gets the stable administrative identifier.</summary>
    public string Id { get; init; }
    /// <summary>Gets the unique profile name.</summary>
    public string Name { get; init; }
    /// <summary>Gets the logical provider index name.</summary>
    public string IndexName { get; init; }
    /// <summary>Gets the provider's registered name.</summary>
    public string ProviderName { get; init; }
    /// <summary>Gets the registered source type.</summary>
    public string Type { get; init; }
    /// <summary>Gets when the profile was created.</summary>
    public DateTime CreatedUtc { get; init; }
}

/// <summary>A bounded page of index identities without private provider properties.</summary>
public sealed class IndexListResponse
{
    /// <summary>Gets the applied one-based page.</summary>
    public int Page { get; init; }
    /// <summary>Gets the applied page size.</summary>
    public int PageSize { get; init; }
    /// <summary>Gets the number of matches before paging.</summary>
    public int TotalCount { get; init; }
    /// <summary>Gets the profiles on this page.</summary>
    public IReadOnlyList<IndexProfileResponse> Items { get; init; } = [];
}

/// <summary>Describes a registered index provider and its source types.</summary>
public sealed class IndexProviderResponse
{
    /// <summary>Gets the provider's stable registered name.</summary>
    public string Name { get; init; }
    /// <summary>Gets its localized display name.</summary>
    public string DisplayName { get; init; }
    /// <summary>Gets source types registered for this provider.</summary>
    public IReadOnlyList<IndexSourceResponse> Sources { get; init; } = [];
}

/// <summary>Describes a provider's registered source without internal service types.</summary>
public sealed class IndexSourceResponse
{
    /// <summary>Gets remote lifecycle actions verified for this provider/source pair.</summary>
    public IReadOnlyList<string> LifecycleActions { get; init; } = [];

    /// <summary>Gets the stable source type.</summary>
    public string Type { get; init; }
    /// <summary>Gets its localized display name.</summary>
    public string DisplayName { get; init; }
    /// <summary>Gets its localized description.</summary>
    public string Description { get; init; }
}

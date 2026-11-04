using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OrchardCore.Tenants.Endpoints.Management;

/// <summary>A complete editable feature-profile definition.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class FeatureProfileDefinition
{
    /// <summary>Gets the immutable tenant-assignment identifier.</summary>
    [Required]
    public string Id { get; init; }

    /// <summary>Gets the editable display name.</summary>
    [Required]
    public string Name { get; init; }

    /// <summary>Gets the ordered rules. Omission clears the rules; null is invalid.</summary>
    public FeatureProfileRuleDefinition[] FeatureRules { get; init; } = [];
}

/// <summary>A registered feature-selection rule and its provider-owned expression.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class FeatureProfileRuleDefinition
{
    /// <summary>Gets the registered rule name.</summary>
    [Required]
    public string Rule { get; init; }

    /// <summary>Gets the expression interpreted by the registered rule delegate.</summary>
    [Required]
    public string Expression { get; init; }
}

/// <summary>Filters and pages feature-profile definitions.</summary>
public sealed class FeatureProfileListRequest
{
    /// <summary>Gets an optional case-insensitive ID or display-name search.</summary>
    public string Search { get; init; }

    /// <summary>Gets the zero-based offset, defaulting to zero.</summary>
    [Range(0, int.MaxValue)]
    public int? Skip { get; init; }

    /// <summary>Gets the page size, defaulting to 50 and limited to 200.</summary>
    [Range(1, 200)]
    public int? Take { get; init; }
}

/// <summary>A bounded page of feature profiles.</summary>
public sealed class FeatureProfileListResponse
{
    /// <summary>Gets the applied offset.</summary>
    public int Skip { get; init; }

    /// <summary>Gets the applied page size.</summary>
    public int Take { get; init; }

    /// <summary>Gets the matching count before paging.</summary>
    public int TotalCount { get; init; }

    /// <summary>Gets the definitions in this page.</summary>
    public IReadOnlyList<FeatureProfileDefinition> Items { get; init; } = [];
}

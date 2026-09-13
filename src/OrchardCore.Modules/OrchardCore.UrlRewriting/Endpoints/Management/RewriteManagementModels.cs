using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OrchardCore.UrlRewriting.Endpoints.Management;

/// <summary>A complete definition for a built-in Rewrite or Redirect source.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class RewriteDefinition
{
    /// <summary>Gets an optional stable identifier for idempotent creation.</summary>
    [MaxLength(128)]
    public string Id { get; init; }
    /// <summary>Gets the display name.</summary>
    [Required]
    public string Name { get; init; }
    /// <summary>Gets the source name: Rewrite or Redirect. Updates cannot change it.</summary>
    [Required]
    public string Source { get; init; }
    /// <summary>Gets the match regular expression.</summary>
    [Required]
    public string Pattern { get; init; }
    /// <summary>Gets the replacement URL expression, including optional capture references.</summary>
    [Required]
    public string SubstitutionPattern { get; init; }
    /// <summary>Gets whether matching ignores case.</summary>
    public bool IsCaseInsensitive { get; init; }
    /// <summary>Gets Append or Drop, defaulting to Append.</summary>
    public string QueryStringPolicy { get; init; } = "Append";
    /// <summary>Gets whether a Rewrite stops subsequent rules. Invalid for Redirect.</summary>
    public bool? SkipFurtherRules { get; init; }
    /// <summary>Gets a Redirect status name, defaulting to Found. Invalid for Rewrite.</summary>
    public string RedirectType { get; init; }
}

/// <summary>A stored rule with an allowlisted definition when its source is supported.</summary>
public sealed class RewriteResponse
{
    /// <summary>Gets the stable rule identifier.</summary>
    public string Id { get; init; }
    /// <summary>Gets the display name.</summary>
    public string Name { get; init; }
    /// <summary>Gets the registered source name.</summary>
    public string Source { get; init; }
    /// <summary>Gets the zero-based stored order.</summary>
    public int Order { get; init; }
    /// <summary>Gets the creation timestamp with the document store's whole-second precision.</summary>
    [JsonConverter(typeof(OrchardCore.Json.Serialization.DateTimeJsonConverter))]
    public DateTime CreatedUtc { get; init; }
    /// <summary>Gets whether the remote contract supports editing this source.</summary>
    public bool IsWritable { get; init; }
    /// <summary>Gets the built-in definition, or null for an opaque extension source.</summary>
    public RewriteDefinition Definition { get; init; }
}

/// <summary>A validation result that does not save or execute the rule.</summary>
public sealed class RewriteValidationResponse
{
    /// <summary>Gets whether the definition can be parsed by its source.</summary>
    public bool IsValid { get; init; }
    /// <summary>Gets field validation messages.</summary>
    public Dictionary<string, string[]> Errors { get; init; } = [];
}

/// <summary>A request to move a stored rule to a zero-based position.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class RewriteMoveRequest
{
    /// <summary>Gets the desired zero-based position in the complete ordered list.</summary>
    [Required]
    [Range(0, int.MaxValue)]
    public int? Position { get; init; }
}

/// <summary>An ordered page of rewrite rules.</summary>
public sealed class RewritePage
{
    /// <summary>Gets the zero-based offset.</summary>
    public int Skip { get; init; }
    /// <summary>Gets the page size.</summary>
    public int Take { get; init; }
    /// <summary>Gets the total count before paging.</summary>
    public int TotalCount { get; init; }
    /// <summary>Gets the rules in runtime order.</summary>
    public RewriteResponse[] Items { get; init; } = [];
}

/// <summary>A registered runtime source and its remote editing support.</summary>
public sealed class RewriteSourceResponse
{
    /// <summary>Gets the technical source name.</summary>
    public string Name { get; init; }
    /// <summary>Gets the localized display name.</summary>
    public string DisplayName { get; init; }
    /// <summary>Gets the localized description.</summary>
    public string Description { get; init; }
    /// <summary>Gets whether the built-in remote definition supports this source.</summary>
    public bool IsWritable { get; init; }
}

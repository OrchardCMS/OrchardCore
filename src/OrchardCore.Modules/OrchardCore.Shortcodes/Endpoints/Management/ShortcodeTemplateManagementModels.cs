using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OrchardCore.Shortcodes.Endpoints.Management;

/// <summary>A complete stored shortcode template definition.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ShortcodeTemplateDefinition
{
    /// <summary>Gets the stable shortcode identifier, stored in invariant lowercase.</summary>
    [Required, MaxLength(256)]
    public string Name { get; init; }
    /// <summary>Gets the required, syntactically valid Liquid template.</summary>
    [Required]
    public string Content { get; init; }
    /// <summary>Gets the optional picker hint.</summary>
    public string Hint { get; init; }
    /// <summary>Gets usage HTML, sanitized by the shared manager before storage.</summary>
    public string Usage { get; init; }
    /// <summary>Gets the optional default shortcode insertion text.</summary>
    public string DefaultValue { get; init; }
    /// <summary>Gets the ordered picker categories. Null is normalized to an empty array.</summary>
    public string[] Categories { get; init; } = [];
}

/// <summary>Filters and pages stored shortcode templates.</summary>
public sealed class ShortcodeTemplateListRequest
{
    /// <summary>Gets an optional case-insensitive name or hint search.</summary>
    public string Search { get; init; }
    /// <summary>Gets the zero-based offset, defaulting to zero.</summary>
    [Range(0, int.MaxValue)]
    public int? Skip { get; init; }
    /// <summary>Gets the page size, defaulting to 50 and limited to 200.</summary>
    [Range(1, 200)]
    public int? Take { get; init; }
}

/// <summary>A page of stored shortcode templates.</summary>
public sealed class ShortcodeTemplateListResponse
{
    /// <summary>Gets the applied offset.</summary>
    public int Skip { get; init; }
    /// <summary>Gets the applied page size.</summary>
    public int Take { get; init; }
    /// <summary>Gets the number of matching templates before paging.</summary>
    public int TotalCount { get; init; }
    /// <summary>Gets the template definitions in this page.</summary>
    public IReadOnlyList<ShortcodeTemplateDefinition> Items { get; init; } = [];
}

/// <summary>Reports validation without persistence or Liquid execution.</summary>
public sealed class ShortcodeTemplateValidationResponse
{
    /// <summary>Gets whether the complete definition is valid.</summary>
    public bool IsValid { get; init; }
    /// <summary>Gets localized errors keyed by request property path.</summary>
    public IDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
}

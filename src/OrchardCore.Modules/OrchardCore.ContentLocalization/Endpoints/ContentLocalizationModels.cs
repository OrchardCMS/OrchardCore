using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OrchardCore.ContentLocalization.Endpoints;

/// <summary>A configured culture to create or reuse for a content item.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class LocalizeContentRequest
{
    /// <summary>The configured culture name; an empty string selects a configured invariant culture.</summary>
    [Required(AllowEmptyStrings = true)]
    public string Culture { get; set; }
}

/// <summary>Safe identity and version information for an authorized localized variant.</summary>
public sealed class ContentLocalizationResponse
{
    /// <summary>The content item identifier.</summary>
    public string ContentItemId { get; init; }
    /// <summary>The selected content version identifier.</summary>
    public string ContentItemVersionId { get; init; }
    /// <summary>The content type identifier.</summary>
    public string ContentType { get; init; }
    /// <summary>The variant's display text.</summary>
    public string DisplayText { get; init; }
    /// <summary>The localization set identifier.</summary>
    public string LocalizationSet { get; init; }
    /// <summary>The variant's culture.</summary>
    public string Culture { get; init; }
    /// <summary>Whether this version is published.</summary>
    public bool Published { get; init; }
    /// <summary>Whether this version is the latest.</summary>
    public bool Latest { get; init; }
}

/// <summary>The created or reused variant; creating a localization never publishes it.</summary>
public sealed class LocalizeContentResponse
{
    /// <summary>The resulting variant's identity and version information.</summary>
    public ContentLocalizationResponse Item { get; init; }
    /// <summary>Whether a new draft was created rather than reusing an existing variant.</summary>
    public bool Created { get; init; }
}

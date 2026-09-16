using System.Security.Claims;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentLocalization.Services;

/// <summary>Shares authorized content localization between the admin action and remote management.</summary>
public interface IContentLocalizationService
{
    /// <summary>
    /// Creates a draft through the localization manager or returns the existing editable variant.
    /// Requires source localization permission and permission to create/edit the content type.
    /// The target must be a configured culture; an empty string represents the invariant culture.
    /// </summary>
    Task<ContentLocalizationResult> LocalizeAsync(ClaimsPrincipal user, string contentItemId, string culture);
}

/// <summary>The outcome of a localization request.</summary>
public enum ContentLocalizationStatus
{
    /// <summary>A new draft was created or an existing variant was reused.</summary>
    Success,
    /// <summary>The source does not exist or has no localization part.</summary>
    NotFound,
    /// <summary>The identity cannot localize the source or edit the target.</summary>
    Forbidden,
    /// <summary>The requested culture is unsupported.</summary>
    Invalid,
    /// <summary>An existing published variant has a latest draft with different set/culture membership.</summary>
    Conflict,
}

/// <summary>A localized item and whether this request created it.</summary>
public sealed class ContentLocalizationResult
{
    /// <summary>The request outcome.</summary>
    public ContentLocalizationStatus Status { get; init; }
    /// <summary>The new draft or existing latest variant, only on success.</summary>
    public ContentItem ContentItem { get; init; }
    /// <summary>Whether a new draft was created.</summary>
    public bool Created { get; init; }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardCore.Localization;

namespace OrchardCore.ContentLocalization.Services;

internal sealed class ContentLocalizationService : IContentLocalizationService
{
    private readonly IContentManager _content;
    private readonly IContentLocalizationManager _localizations;
    private readonly ILocalizationService _cultures;
    private readonly IAuthorizationService _authorization;

    public ContentLocalizationService(IContentManager content, IContentLocalizationManager localizations,
        ILocalizationService cultures, IAuthorizationService authorization)
    {
        _content = content;
        _localizations = localizations;
        _cultures = cultures;
        _authorization = authorization;
    }

    public async Task<ContentLocalizationResult> LocalizeAsync(ClaimsPrincipal user, string contentItemId, string culture)
    {
        var source = await _content.GetAsync(contentItemId, VersionOptions.Latest);
        if (source is null)
        {
            return new() { Status = ContentLocalizationStatus.NotFound };
        }
        if (!await _authorization.AuthorizeAsync(user, ContentLocalizationPermissions.LocalizeContent, source)
            || !await _authorization.AuthorizeContentTypeAsync(user, CommonPermissions.EditContent, source.ContentType, user.FindFirstValue(ClaimTypes.NameIdentifier)))
        {
            return new() { Status = ContentLocalizationStatus.Forbidden };
        }
        if (!source.TryGet<LocalizationPart>(out var part))
        {
            return new() { Status = ContentLocalizationStatus.NotFound };
        }
        culture ??= string.Empty;
        var canonicalCulture = (await _cultures.GetSupportedCulturesAsync()).FirstOrDefault(value => string.Equals(value, culture, StringComparison.OrdinalIgnoreCase));
        if (canonicalCulture is null)
        {
            return new() { Status = ContentLocalizationStatus.Invalid };
        }
        ContentItem existing = null;
        if (!string.IsNullOrEmpty(part.LocalizationSet))
        {
            existing = await _localizations.GetContentItemAsync(part.LocalizationSet, canonicalCulture);
        }
        if (existing is not null)
        {
            // The manager can return a published version when a separate draft exists.
            existing = await _content.GetAsync(existing.ContentItemId, VersionOptions.Latest);
            if (existing is null || !await _authorization.AuthorizeAsync(user, CommonPermissions.EditContent, existing))
            {
                return new() { Status = ContentLocalizationStatus.Forbidden };
            }
            if (!existing.TryGet<LocalizationPart>(out var latestPart) || latestPart.LocalizationSet != part.LocalizationSet
                || !string.Equals(latestPart.Culture, canonicalCulture, StringComparison.OrdinalIgnoreCase))
            {
                return new() { Status = ContentLocalizationStatus.Conflict };
            }
            return new() { ContentItem = existing };
        }
        var localized = await _localizations.LocalizeAsync(source, canonicalCulture);
        return new() { ContentItem = localized, Created = localized.ContentItemId != source.ContentItemId };
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OrchardCore.Alias.Indexes;
using OrchardCore.Alias.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace OrchardCore.Alias.Services;

public sealed class AliasImportErrorProvider : IContentImportErrorProvider
{
    private readonly ISession _session;
    private readonly IStringLocalizer S;

    public AliasImportErrorProvider(ISession session, IStringLocalizer<AliasImportErrorProvider> localizer)
    {
        _session = session;
        S = localizer;
    }

    public async ValueTask<string> GetDetailsAsync(ContentItem importingItem, IReadOnlyList<ValidationResult> errors)
    {
        if (!HasAliasValidationError(errors))
        {
            return string.Empty;
        }

        if (!importingItem.TryGet<AliasPart>(out var aliasPart) || string.IsNullOrWhiteSpace(aliasPart?.Alias))
        {
            return string.Empty;
        }

        var alias = aliasPart.Alias;
        var normalizedAlias = alias.ToLowerInvariant();

        var conflict = await _session.QueryIndex<AliasPartIndex>(x => x.Alias == normalizedAlias && x.ContentItemId != importingItem.ContentItemId)
            .FirstOrDefaultAsync();

        if (conflict == null)
        {
            return string.Empty;
        }

        var targetIndex = await _session.QueryIndex<ContentItemIndex>(x =>
            x.ContentItemId == conflict.ContentItemId && (x.Latest || x.Published)).FirstOrDefaultAsync();

        var targetInfo = string.IsNullOrWhiteSpace(targetIndex?.DisplayText)
            ? S["ContentItemId '{0}'", conflict.ContentItemId].Value
            : S["ContentItemId '{0}' (DisplayText: '{1}')", conflict.ContentItemId, targetIndex.DisplayText].Value;

        return S["Alias conflict: '{0}' is already used by {1}.", alias, targetInfo].Value;
    }

    private static bool HasAliasValidationError(IReadOnlyList<ValidationResult> errors)
    {
        if (errors == null || errors.Count == 0)
        {
            return false;
        }

        return errors.Any(error => error.MemberNames?.Contains(nameof(AliasPart.Alias), StringComparer.OrdinalIgnoreCase) == true);
    }
}

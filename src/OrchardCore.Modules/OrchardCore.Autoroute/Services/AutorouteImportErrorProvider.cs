using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OrchardCore.Autoroute.Core.Indexes;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Autoroute.Services;

public sealed class AutorouteImportErrorProvider : IContentImportErrorProvider
{
    private readonly ISession _session;
    private readonly IStringLocalizer S;

    public AutorouteImportErrorProvider(ISession session, IStringLocalizer<AutorouteImportErrorProvider> localizer)
    {
        _session = session;
        S = localizer;
    }

    public async ValueTask<string> GetDetailsAsync(ContentItem importingItem, IReadOnlyList<ValidationResult> errors)
    {
        if (!HasPathValidationError(errors))
        {
            return string.Empty;
        }

        if (!importingItem.TryGet<AutoroutePart>(out var autoroutePart) || string.IsNullOrWhiteSpace(autoroutePart?.Path))
        {
            return string.Empty;
        }

        var path = autoroutePart.Path;
        var normalized = path.Trim('/');
        var paths = new[] { normalized, "/" + normalized, normalized + "/", "/" + normalized + "/" };

        var conflict = await _session.QueryIndex<AutoroutePartIndex>(x =>
            (x.Published || x.Latest) && x.Path.IsIn(paths)).FirstOrDefaultAsync();

        if (conflict == null)
        {
            return string.Empty;
        }

        if (string.Equals(conflict.ContentItemId, importingItem.ContentItemId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(conflict.ContainedContentItemId, importingItem.ContentItemId, StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        var targetContentItemId = conflict.ContainedContentItemId ?? conflict.ContentItemId;

        var targetIndex = await _session.QueryIndex<ContentItemIndex>(x =>
            x.ContentItemId == targetContentItemId && (x.Latest || x.Published)).FirstOrDefaultAsync();

        var targetInfo = string.IsNullOrWhiteSpace(targetIndex?.DisplayText)
            ? S["ContentItemId '{0}'", targetContentItemId].Value
            : S["ContentItemId '{0}' (DisplayText: '{1}')", targetContentItemId, targetIndex.DisplayText].Value;

        return S["Permalink conflict: '{0}' is already used by {1} (Permalink: '{2}').", path, targetInfo, conflict.Path].Value;
    }

    private static bool HasPathValidationError(IReadOnlyList<ValidationResult> errors)
    {
        if (errors == null || errors.Count == 0)
        {
            return false;
        }

        return errors.Any(error => error.MemberNames?.Contains(nameof(AutoroutePart.Path), StringComparer.OrdinalIgnoreCase) == true);
    }
}

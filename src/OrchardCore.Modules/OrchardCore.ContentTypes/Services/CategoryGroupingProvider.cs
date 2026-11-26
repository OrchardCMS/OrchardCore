using System.Collections;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.Models;

namespace OrchardCore.ContentTypes.Services;

internal sealed class CategoryGroupingProvider : IAdminListGroupingProvider
{
    private readonly IStringLocalizer S;

    public CategoryGroupingProvider(IStringLocalizer<CategoryGroupingProvider> localizer)
    {
        S = localizer;
    }

    public string Name => "Category";
    public LocalizedString DisplayName => S["Category"];

    public Task<IReadOnlyList<AdminListGroup>> GroupAsync(IEnumerable source, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<AdminListGroup>>(source.Cast<ContentTypeEntry>()
            .GroupBy(item => item.TypeDefinition.GetSettings<ContentTypeSettings>().Category ?? string.Empty, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.CurrentCultureIgnoreCase)
            .Select(group => new AdminListGroup
            {
                Key = group.Key,
                Title = string.IsNullOrWhiteSpace(group.Key) ? S["Uncategorized"].Value : group.Key,
                Source = group.ToArray(),
                TotalCount = group.Count(),
            }).ToArray());
}

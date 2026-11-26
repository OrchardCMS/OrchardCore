using System.Collections;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentTypes;
using OrchardCore.Localization.Data;

namespace OrchardCore.Contents.Services;

internal sealed class ContentTypeGroupingProvider : IAdminListGroupingProvider
{
    private readonly IContentDefinitionManager _definitions;
    private readonly IDataLocalizer D;
    private readonly IStringLocalizer S;

    public ContentTypeGroupingProvider(IContentDefinitionManager definitions, IDataLocalizer dataLocalizer, IStringLocalizer<ContentTypeGroupingProvider> localizer)
    {
        _definitions = definitions;
        D = dataLocalizer;
        S = localizer;
    }

    public string Name => "ContentType";
    public LocalizedString DisplayName => S["Content type"];

    public async Task<IReadOnlyList<AdminListGroup>> GroupAsync(IEnumerable source, CancellationToken cancellationToken = default)
    {
        var definitions = (await _definitions.ListTypeDefinitionsAsync()).ToDictionary(type => type.Name, StringComparer.Ordinal);
        return source.Cast<ContentItemIndex>().GroupBy(item => item.ContentType, StringComparer.Ordinal)
            .Select(group => new AdminListGroup
            {
                Key = group.Key,
                Title = D[definitions.TryGetValue(group.Key, out var type) ? type.DisplayName : group.Key, DataLocalizationContext.ContentType].Value,
                Source = group.ToArray(),
                TotalCount = group.Count(),
            }).ToArray();
    }
}

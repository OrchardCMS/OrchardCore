using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace OrchardCore.ContentLocalization.Indexes;

/// <summary>
/// Records localization membership for every version, including historical versions.
/// </summary>
public sealed class LocalizationSetIndex : MapIndex
{
    /// <summary>
    /// The indexed version's document identifier.
    /// </summary>
    public long DocumentId { get; set; }

    /// <summary>
    /// The localization set of this version.
    /// </summary>
    public string LocalizationSet { get; set; }
}

internal sealed class LocalizationSetIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context)
        => context.For<LocalizationSetIndex>()
            .ExclusiveToDocumentType()
            .SingleRecordPerDocument()
            .Map(item => item.TryGet<LocalizationPart>(out var part) && !string.IsNullOrEmpty(part.LocalizationSet)
                ? new LocalizationSetIndex { LocalizationSet = part.LocalizationSet }
                : null);
}

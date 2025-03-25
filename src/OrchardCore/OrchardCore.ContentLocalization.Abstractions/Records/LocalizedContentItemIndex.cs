using YesSql.Indexes;

namespace OrchardCore.ContentLocalization.Records;

public class LocalizedContentItemIndex : MapIndex
{
    public long DocumentId { get; set; }
    public string ContentItemId { get; set; }
    public string LocalizationSet { get; set; }
    public string Culture { get; set; }
    public bool Published { get; set; }
    public bool Latest { get; set; }
}

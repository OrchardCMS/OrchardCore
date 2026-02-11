using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.ContentTypes.Shapes;

public class ContentCardShape
{
    public IUpdateModel Updater { get; set; }
    public string CollectionShapeType { get; set; }
    public ContentItem ContentItem { get; set; }
    public bool BuildEditor { get; set; }
    public string ParentContentType { get; set; }
    public string CollectionPartName { get; set; }
    public IEnumerable<ContentTypeDefinition> ContainedContentTypes { get; set; }
    public string TargetId { get; set; }
    public bool Inline { get; set; }
    public bool CanMove { get; set; }
    public bool CanDelete { get; set; }
    public bool CanInsert { get; set; } = true;
    public string PrefixValue { get; set; }
    public string PrefixesName { get; set; }
    public string PrefixesId { get; set; }
    public string ContentTypesName { get; set; }
    public string ContentTypesId { get; set; }
    public string ContentItemsName { get; set; }
    public string ContentItemsId { get; set; }
    public string ZonesName { get; set; }
    public string ZonesId { get; set; }
    public string ZoneValue { get; set; }
    public int? ColumnSize { get; set; }
    public object CollectionShape { get; set; }
    public string HtmlFieldPrefix { get; set; }
    public string ContentTypeValue { get; set; }
    public IShape ContentEditor { get; set; }
    public IShape ContentPreview { get; set; }
    public IShape Footer { get; set; }
    public string DisplayType { get; set; }
}

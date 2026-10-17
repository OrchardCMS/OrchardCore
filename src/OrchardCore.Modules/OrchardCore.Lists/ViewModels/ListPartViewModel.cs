using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.ViewModels;

public class ListPartViewModel
{
    public ListPartFilterViewModel ListPartFilterViewModel { get; set; }
    public ListPart ListPart { get; set; }
    public IEnumerable<ContentItem> ContentItems { get; set; }
    public IEnumerable<ContentTypeDefinition> ContainedContentTypeDefinitions { get; set; }
    public BuildPartDisplayContext Context { get; set; }
    public dynamic Pager { get; set; }
    public bool EnableOrdering { get; set; }

    /// <summary>
    /// Whether the contained items can be reordered by dragging them on this rendering: ordering is enabled on
    /// the list, there is more than one item, and the items are neither filtered by status nor searched.
    /// </summary>
    public bool IsOrdering { get; set; }

    /// <summary>
    /// The item data and rendering options for the contained-items admin list.
    /// </summary>
    public AdminListContext List { get; set; }
}

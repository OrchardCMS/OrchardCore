using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Widgets.Models;

// A content item with this part can have widget instances.
public class WidgetsListPart : ContentPart
{
    [Description("The widget content items grouped by zone name.")]
    public Dictionary<string, List<ContentItem>> Widgets { get; set; } = [];
}

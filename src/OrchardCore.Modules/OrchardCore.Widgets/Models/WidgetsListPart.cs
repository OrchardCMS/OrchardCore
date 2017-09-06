using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.Widgets.Models
{
    // A content item with this part can have widget instances.
    public class WidgetsListPart : ContentPart
    {
        public Dictionary<string, List<ContentItem>> Widgets { get; } = new Dictionary<string, List<ContentItem>>();
    }
}

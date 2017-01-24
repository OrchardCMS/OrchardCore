using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Widgets.Models
{
    // A content item with this part can have widget instances.
    public class WidgetsListPart : ContentPart
    {
        public List<ContentItem> Widgets { get; } = new List<ContentItem>();
    }
}

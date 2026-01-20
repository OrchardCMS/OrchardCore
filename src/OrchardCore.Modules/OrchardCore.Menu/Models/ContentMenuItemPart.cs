using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.Menu.Models
{
    public class ContentMenuItemPart : ContentPart
    {
        [Obsolete("This property is obsolete and will be removed in a future version. Use 'DisplayText'")]
        public string Name { get; set; }
    }
}

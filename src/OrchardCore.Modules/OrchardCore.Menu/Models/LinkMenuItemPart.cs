using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.Menu.Models
{
    public class LinkMenuItemPart : ContentPart
    {
        [Obsolete("This property is obsolete and will be removed in a future version. Use 'DisplayText'")]
        public string Name { get; set; }

        /// <summary>
        /// The url of the link to create.
        /// </summary>
        public string Url { get; set; }
    }
}

using OrchardCore.ContentManagement;

namespace OrchardCore.Menu.Models
{
    public class LinkMenuItemPart : ContentPart
    {
        /// <summary>
        /// The name of the link
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The url of the link to create.
        /// </summary>
        public string Url { get; set; }
    }
}

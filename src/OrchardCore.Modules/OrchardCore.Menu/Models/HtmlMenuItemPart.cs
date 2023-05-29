using OrchardCore.ContentManagement;

namespace OrchardCore.Menu.Models
{
    public class HtmlMenuItemPart : ContentPart
    {
        /// <summary>
        /// The url of the link to create.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The raw html to display for this link.
        /// </summary>
        public string Html { get; set; }
    }
}

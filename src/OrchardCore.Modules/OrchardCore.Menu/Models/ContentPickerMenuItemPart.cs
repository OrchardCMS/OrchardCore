using OrchardCore.ContentManagement;

namespace OrchardCore.Menu.Models
{
    public class ContentPickerMenuItemPart : ContentPart
    {
        /// <summary>
        /// The name of the link
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Content Picker ItemIds
        /// </summary>
        public string ContentItemIds { get; set; }

        /// <summary>
        /// The url of the content picker.
        /// </summary>
        public string Url { get; set; }
    }
}

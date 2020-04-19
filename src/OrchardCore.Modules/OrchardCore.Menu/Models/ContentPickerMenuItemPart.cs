using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        [BindProperty(Name = "ContentItemIds")]
        public string ContentItemId { get; set; }
    }
}

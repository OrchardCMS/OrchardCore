using Orchard.ContentManagement;

namespace Orchard.Identity.Models
{
    public class IdentityPart : ContentPart
    {
        /// <summary>
        /// A friendly name that can be used as a display text.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Get or sets the globally unique identifier.
        /// </summary>
        public string Identity { get; set; }
    }
}

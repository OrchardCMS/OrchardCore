using Orchard.ContentManagement;

namespace Orchard.Identity.Models
{
    public class IdentityPart : ContentPart
    {
        /// <summary>
        /// Get or sets the globally unique identifier.
        /// </summary>
        public string Identifier { get; set; }
    }
}

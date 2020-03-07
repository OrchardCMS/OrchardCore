using OrchardCore.ContentManagement;

namespace OrchardCore.Autoroute.Models
{
    public class AutoroutePart : ContentPart
    {
        public string Path { get; set; }

        /// <summary>
        /// Whether to make the content item the homepage once it's published.
        /// </summary>
        public bool SetHomepage { get; set; }

        /// <summary>
        /// Whether to disable autoroute generation for this content item.
        /// When disabled the path is no longer validated, or generated.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Whether to route content items contained within the content item.
        /// </summary>
        public bool RouteContainedItems { get; set; }

        /// <summary>
        /// When this content item is contained within another is the route relative to the container route.
        /// </summary>
        public bool Absolute { get; set; }
    }
}

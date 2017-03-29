using Orchard.ContentManagement;

namespace Orchard.Autoroute.Model
{
    public class AutoroutePart : ContentPart
    {
        public string Path { get; set; }

        /// <summary>
        /// Whether to make the content item the homepage once it's published.
        /// </summary>
        public bool SetHomepage { get; set; }
    }
}

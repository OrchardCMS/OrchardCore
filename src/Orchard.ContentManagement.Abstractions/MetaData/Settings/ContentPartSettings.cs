namespace Orchard.ContentManagement.Metadata.Settings
{
    public class ContentPartSettings
    {
        /// <summary>
        /// Gets or sets whether this part can be manually attached to a content type.
        /// </summary>
        public bool Attachable { get; set; }

        /// <summary>
        /// Gets or sets whether the part can be attached multiple times to a content type.
        /// </summary>
        public bool Reusable { get; set; }

        /// <summary>
        /// Gets or sets the displayed name of the part.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gest or set the description of the part.
        /// </summary>
        public string Description { get; set; }
    }
}
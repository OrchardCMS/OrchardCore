namespace OrchardCore.ContentManagement.Metadata.Settings
{
    public class ContentTypeSettings
    {
        /// <summary>
        /// Used to determine if an instance of this content type can be created through the UI
        /// </summary>
        public bool Creatable { get; set; }

        /// <summary>
        /// Used to determine if an instance of this content type can be listed in the contents page
        /// </summary>
        public bool Listable { get; set; }

        /// <summary>
        /// Used to determine if this content type supports draft versions
        /// </summary>
        public bool Draftable { get; set; }

        /// <summary>
        /// Used to determine if this content type supports versioning
        /// </summary>
        public bool Versionable { get; set; }

        /// <summary>
        /// Defines the stereotype of the type
        /// </summary>
        public string Stereotype { get; set; }

        /// <summary>
        /// Used to determine if this content type supports custom permissions
        /// </summary>
        public bool Securable { get; set; }

        /// <summary>
        /// Gets or sets the description name of this content type.
        /// </summary>
        public string Description { get; set; }
    }
}

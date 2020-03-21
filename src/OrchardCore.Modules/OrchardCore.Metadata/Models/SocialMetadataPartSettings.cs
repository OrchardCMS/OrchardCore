namespace OrchardCore.Metadata.Models
{
    public class SocialMetadataPartSettings
    {
        /// <summary>
        /// Is Open Graph tags to be used for this content type.
        /// </summary>
        public bool SupportOpenGraph { get; set; }

        /// <summary>
        /// Is Twitter Cards metadata to be used for this content type.
        /// </summary>
        public bool SupportTwitterCards { get; set; }
    }
}

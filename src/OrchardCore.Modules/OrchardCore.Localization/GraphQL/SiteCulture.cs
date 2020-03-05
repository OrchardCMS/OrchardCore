namespace OrchardCore.Localization.GraphQL
{
    /// <summary>
    /// Represents a culture for the site.
    /// </summary>
    public class SiteCulture
    {
        /// <summary>
        /// Gets or sets a culture.
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// Gets or sets whether the <see cref="Culture"/> is used as default one.
        /// </summary>
        public bool IsDefault { get; set; }
    }
}

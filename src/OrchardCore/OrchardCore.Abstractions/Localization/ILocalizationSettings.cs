namespace OrchardCore.Localization
{
    public interface ILocalizationSettings
    {
        /// <summary>
        /// Gets or sets the default culture of the site.
        /// </summary>
        string DefaultCulture { get; set; }

        /// <summary>
        /// Gets or sets all the supported cultures of the site. It also contains the default culture.
        /// </summary>
        string[] SupportedCultures { get; set; }
    }
}

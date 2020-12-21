using System;

namespace OrchardCore.Sitemaps.Models
{
    /// <summary>
    /// A sitemap source for managing custom url.
    /// </summary>
    public class CustomPathSitemapSource : SitemapSource
    {
        public static char[] InvalidCharactersForPath = ":?#[]@!$&'()*+,.;=<>\\|%".ToCharArray();
        public const int MaxPathLength = 1024;

        /// <summary>
        /// Gets and sets the custom url.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets and sets last update date time. Updated automatically by te system.
        /// </summary>
        public DateTime? LastUpdate { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets and sets the frequency to apply to sitemap entries.
        /// </summary>
        public ChangeFrequency ChangeFrequency { get; set; }

        // Handle as int, and convert to float, when building, to support localization.
        public int Priority { get; set; } = 5;
    }
}

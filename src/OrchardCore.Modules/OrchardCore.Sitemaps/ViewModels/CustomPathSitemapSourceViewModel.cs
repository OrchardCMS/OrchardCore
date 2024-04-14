using System.ComponentModel.DataAnnotations;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.ViewModels
{
    /// <summary>
    /// A sitemap source for managing custom url.
    /// </summary>
    public class CustomPathSitemapSourceViewModel
    {
        /// <summary>
        /// Gets and sets the custom url.
        /// </summary>
        [Required]
        public string Path { get; set; }

        /// <summary>
        /// Gets and sets the frequency to apply to sitemap entries.
        /// </summary>
        public ChangeFrequency ChangeFrequency { get; set; }

        // Handle as int, and convert to float, when building, to support localization.
        public int Priority { get; set; } = 5;
    }
}

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OrchardCore.Sitemaps.Models
{
    /// <summary>
    /// Implement this class to define a type of sitemap.
    /// </summary>
    public abstract class Sitemap
    {
        /// <summary>
        /// Unique identifier for sitemap.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the sitemap.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// When false sitemap will not be included in routing.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Sitemap path.
        /// </summary>
        [Required]
        public string Path { get; set; }

        /// <summary>
        /// Can sitemap be contained by another sitemap.
        /// </summary>
        public virtual bool IsContainable => true;
    }
}

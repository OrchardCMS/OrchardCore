using System.ComponentModel;

namespace OrchardCore.Autoroute.Models
{
    public class AutoroutePartSettings
    {
        /// <summary>
        /// Gets or sets whether a user can define a custom path.
        /// </summary>
        public bool AllowCustomPath { get; set; }

        /// <summary>
        /// The pattern used to build the Path.
        /// </summary>
        [DefaultValue("{{ ContentItem.DisplayText | slugify }}")]
        public string Pattern { get; set; } = "{{ ContentItem.DisplayText | slugify }}";

        /// <summary>
        /// Whether to display an option to set the content item as the homepage.
        /// </summary>
        public bool ShowHomepageOption { get; set; }

        /// <summary>
        /// Whether a user can request a new path if some data has changed.
        /// </summary>
        public bool AllowUpdatePath { get; set; }
    }
}
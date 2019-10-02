using System.ComponentModel;

namespace OrchardCore.Title.Models
{
    public class TitlePartSettings
    {
        /// <summary>
        /// Gets or sets whether a user can define a custom title
        /// </summary>
        [DefaultValue(true)]
        public bool AllowCustomTitle { get; set; } = true;

        /// <summary>
        /// The pattern used to build the Title.
        /// </summary>
        public string Pattern { get; set; }
    }
}
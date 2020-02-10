using System.ComponentModel;

namespace OrchardCore.Contents.Models
{
    public class FullTextAspectSettings
    {
        /// <summary>
        /// Whether the content type should use a custom template to add content to the full-text aspect.
        /// </summary>
        public bool IncludeFullTextTemplate { get; set; }

        /// <summary>
        /// The template used for the full-text aspect.
        /// </summary>
        public string FullTextTemplate { get; set; }

        /// <summary>
        /// Whether the body aspect should be added to the full-text aspect.
        /// </summary>
        [DefaultValue(true)]
        public bool IncludeBodyAspect { get; set; } = true;

        /// <summary>
        /// Whether the display text should be added to the full-text aspect.
        /// </summary>
        [DefaultValue(true)]
        public bool IncludeDisplayText { get; set; } = true;
    }
}

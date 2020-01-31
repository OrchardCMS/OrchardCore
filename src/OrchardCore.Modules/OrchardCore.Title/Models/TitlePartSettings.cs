using System.ComponentModel;

namespace OrchardCore.Title.Models
{
    public enum TitlePartOptions
    {
        Editable,
        GeneratedDisabled,
        GeneratedHidden,
    }
    public class TitlePartSettings
    {
        /// <summary>
        /// Gets or sets whether a user can define a custom title
        /// </summary>
        [DefaultValue(TitlePartOptions.Editable)]
        public TitlePartOptions Options { get; set; } = TitlePartOptions.Editable;

        /// <summary>
        /// Gets or sets if value must be required
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// The pattern used to build the Title.
        /// </summary>
        public string Pattern { get; set; }
    }
}
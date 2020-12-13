using System.ComponentModel;

namespace OrchardCore.Title.Models
{
    public enum TitlePartOptions
    {
        Editable,
        GeneratedDisabled,
        GeneratedHidden,
        EditableRequired
    }

    public class TitlePartSettings
    {
        /// <summary>
        /// Gets or sets whether a user can define a custom title
        /// </summary>
        [DefaultValue(TitlePartOptions.Editable)]
        public TitlePartOptions Options { get; set; } = TitlePartOptions.Editable;

        /// <summary>
        /// The pattern used to build the Title.
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// Gets or sets whether to render the title in display views.
        /// </summary>
        [DefaultValue(true)]
        public bool RenderTitle { get; set; } = true;
    }
}

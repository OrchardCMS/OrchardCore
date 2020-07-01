using System.ComponentModel;

namespace OrchardCore.Taxonomies.Settings
{
    /// <summary>
    /// When transitioning a tag to a taxonomy editor these settings will need to be reset.
    /// </summary>
    // Despite being similar settings, we want different defaults when creating a tag editor.
    public class TaxonomyFieldTagsEditorSettings
    {
        /// <summary>
        /// Whether the field allows the user to add new tags to the taxonomy
        /// </summary>
        [DefaultValue(true)]
        public bool Open { get; set; } = true;
    }
}

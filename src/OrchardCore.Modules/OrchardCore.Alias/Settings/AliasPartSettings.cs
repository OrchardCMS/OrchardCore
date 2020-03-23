using System.ComponentModel;

namespace OrchardCore.Alias.Settings
{
    public enum AliasPartOptions
    {
        Editable,
        GeneratedDisabled
    }

    public class AliasPartSettings
    {
        [DefaultValue("{{ Model.ContentItem.DisplayText | slugify }}")]
        public string Pattern { get; set; } = "{{ Model.ContentItem.DisplayText | slugify }}";

        /// <summary>
        /// Gets or sets whether a user can define a custom alias
        /// </summary>
        [DefaultValue(AliasPartOptions.Editable)]
        public AliasPartOptions Options { get; set; } = AliasPartOptions.Editable;
    }
}

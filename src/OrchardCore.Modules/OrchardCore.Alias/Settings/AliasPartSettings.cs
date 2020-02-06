using System.ComponentModel;

namespace OrchardCore.Alias.Settings
{
    public class AliasPartSettings
    {
        [DefaultValue("{{ Model.ContentItem.DisplayText | slugify }}")]
        public string Pattern { get; set; } = "{{ Model.ContentItem.DisplayText | slugify }}";
    }
}

using System.ComponentModel;

namespace OrchardCore.Alias.Settings
{
    public class AliasPartSettings
    {
        [DefaultValue("{{ ContentItem.DisplayText | slugify }}")]
        public string Pattern { get; set; } = "{{ ContentItem.DisplayText | slugify }}";
    }
}

using System.ComponentModel;

namespace OrchardCore.Markdown.Settings
{
    public class MarkdownBodyPartSettings
    {
        [DefaultValue(true)]
        public bool SanitizeHtml { get; set; } = true;
    }
}

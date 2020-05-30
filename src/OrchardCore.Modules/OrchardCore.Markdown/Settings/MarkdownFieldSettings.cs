using System.ComponentModel;

namespace OrchardCore.Markdown.Settings
{
    public class MarkdownFieldSettings
    {
        [DefaultValue(true)]
        public bool SanitizeHtml { get; set; } = true;
        public string Hint { get; set; }
    }
}

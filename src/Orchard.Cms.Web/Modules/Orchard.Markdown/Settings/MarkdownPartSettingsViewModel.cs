using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Orchard.Markdown.Settings
{
    public class MarkdownPartSettingsViewModel
    {
        public bool RenderTokens { get; set; }

        public string Editor { get; set; }

        [BindNever]
        public MarkdownPartSettings MarkdownPartSettings { get; set; }
    }
}

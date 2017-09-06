using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Markdown.Settings
{
    public class MarkdownPartSettingsViewModel
    {
        public string Editor { get; set; }

        [BindNever]
        public MarkdownPartSettings MarkdownPartSettings { get; set; }
    }
}

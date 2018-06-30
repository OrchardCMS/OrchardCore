using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Markdown.Settings
{
    public class MarkdownBodyPartSettingsViewModel
    {
        public string Editor { get; set; }

        [BindNever]
        public MarkdownBodyPartSettings MarkdownBodyPartSettings { get; set; }
    }
}

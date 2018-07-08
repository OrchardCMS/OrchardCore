using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.Markdown.Model;
using OrchardCore.Markdown.Settings;

namespace OrchardCore.Markdown.ViewModels
{
    public class MarkdownBodyPartViewModel
    {
        public string Source { get; set; }
        public string Html { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public MarkdownBodyPart MarkdownBodyPart { get; set; }

        [BindNever]
        public MarkdownBodyPartSettings TypePartSettings { get; set; }
    }
}

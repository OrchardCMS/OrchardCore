using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.Markdown.Model;
using OrchardCore.Markdown.Settings;

namespace OrchardCore.Markdown.ViewModels
{
    public class MarkdownPartViewModel
    {
        public string Markdown { get; set; }
        public string Html { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public MarkdownPart MarkdownPart { get; set; }

        [BindNever]
        public MarkdownPartSettings TypePartSettings { get; set; }
    }
}

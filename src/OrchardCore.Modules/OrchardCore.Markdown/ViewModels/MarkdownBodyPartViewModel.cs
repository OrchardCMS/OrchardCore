using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Markdown.Models;

namespace OrchardCore.Markdown.ViewModels
{
    public class MarkdownBodyPartViewModel
    {
        public string Markdown { get; set; }
        public string Html { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public MarkdownBodyPart MarkdownBodyPart { get; set; }

        [BindNever]
        public ContentTypePartDefinition TypePartDefinition { get; set; }
    }
}

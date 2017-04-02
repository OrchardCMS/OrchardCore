using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.Markdown.Model;
using Orchard.Markdown.Settings;

namespace Orchard.Markdown.ViewModels
{
    public class MarkdownPartViewModel
    {
        public string Markdown { get; set; }

        [BindNever]
        public MarkdownPart MarkdownPart { get; set; }

        [BindNever]
        public MarkdownPartSettings TypePartSettings { get; set; }
    }
}

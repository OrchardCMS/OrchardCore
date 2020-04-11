using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.ContentPreview.Models;

namespace OrchardCore.ContentPreview.ViewModels
{
    public class PreviewPartViewModel
    {
        public string Pattern { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public PreviewPart PreviewPart { get; set; }
    }
}

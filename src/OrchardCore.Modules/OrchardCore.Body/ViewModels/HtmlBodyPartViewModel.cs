using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Body.Model;
using OrchardCore.Body.Settings;
using OrchardCore.ContentManagement;

namespace OrchardCore.Body.ViewModels
{
    public class HtmlBodyPartViewModel
    {
        public string Body { get; set; }
        public string Html { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; } 

        [BindNever]
        public HtmlBodyPart HtmlBodyPart { get; set; }

        [BindNever]
        public HtmlBodyPartSettings TypePartSettings { get; set; }
    }
}

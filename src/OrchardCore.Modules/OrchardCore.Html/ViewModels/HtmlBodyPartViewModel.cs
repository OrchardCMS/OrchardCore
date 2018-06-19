using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Html.Model;
using OrchardCore.Html.Settings;
using OrchardCore.ContentManagement;

namespace OrchardCore.Html.ViewModels
{
    public class HtmlBodyPartViewModel
    {
        public string Source { get; set; }
        public string Html { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; } 

        [BindNever]
        public HtmlBodyPart HtmlBodyPart { get; set; }

        [BindNever]
        public HtmlBodyPartSettings TypePartSettings { get; set; }
    }
}

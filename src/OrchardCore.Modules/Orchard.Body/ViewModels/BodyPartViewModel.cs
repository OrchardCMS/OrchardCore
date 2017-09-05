using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Body.Model;
using OrchardCore.Body.Settings;
using OrchardCore.ContentManagement;

namespace OrchardCore.Body.ViewModels
{
    public class BodyPartViewModel
    {
        public string Body { get; set; }
        public string Html { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; } 

        [BindNever]
        public BodyPart BodyPart { get; set; }

        [BindNever]
        public BodyPartSettings TypePartSettings { get; set; }
    }
}

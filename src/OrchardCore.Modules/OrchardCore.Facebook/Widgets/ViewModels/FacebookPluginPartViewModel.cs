using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.Facebook.Widgets.Models;
using OrchardCore.Facebook.Widgets.Settings;

namespace OrchardCore.Facebook.Widgets.ViewModels
{
    public class FacebookPluginPartViewModel
    {
        public string Liquid { get; set; }
        public string Html { get; set; }

        [BindNever]
        public FacebookPluginPartSettings Settings { get; set; }

        [BindNever]
        public FacebookPluginPart FacebookPluginPart { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }
    }
}

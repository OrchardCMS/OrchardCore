using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Html.Settings
{
    public class HtmlBodyPartSettingsViewModel
    {
        public string Editor { get; set; }

        [BindNever]
        public HtmlBodyPartSettings HtmlBodyPartSettings { get; set; }
    }
}

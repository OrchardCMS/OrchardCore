using Microsoft.AspNetCore.Mvc.Localization;

namespace OrchardCore.Settings.ViewModels
{
    public class SiteSetting
    {
        public SiteSetting(string name, LocalizedHtmlString description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; set; }
        public LocalizedHtmlString Description { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Facebook.Widgets.Settings
{
    public class FacebookPluginPartSettingsViewModel
    {
        public string Liquid { get; set; }
        [BindNever]
        public FacebookPluginPartSettings FacebookPluginPartSettings { get; set; }
    }
}

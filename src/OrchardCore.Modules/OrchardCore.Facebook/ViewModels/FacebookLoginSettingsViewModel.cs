using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Facebook.ViewModels
{
    public class FacebookLoginSettingsViewModel : FacebookCoreSettingsViewModel
    {
        [RegularExpression(@"\/[-A-Za-z0-9+&@#\/%?=~_|!:,.;]+[-A-Za-z0-9+&@#\/%=~_|]", ErrorMessage = "Invalid path")]
        public string CallbackPath { get; set; }
    }
}

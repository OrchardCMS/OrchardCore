using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Twitter.ViewModels
{
    public class TwitterSigninSettingsViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Consumer Key is required")]
        public string ConsumerKey { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Consumer Secret is required")]
        public string ConsumerSecret { get; set; }

        [RegularExpression(@"\/[-A-Za-z0-9+&@#\/%?=~_|!:,.;]+[-A-Za-z0-9+&@#\/%=~_|]", ErrorMessage = "Invalid path")]
        public string CallbackPath { get; set; }
    }
}

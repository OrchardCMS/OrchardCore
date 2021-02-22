using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Twitter.ViewModels
{
    public class TwitterSettingsViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "API key is required")]
        public string APIKey { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "API secret key is required")]
        public string APISecretKey { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Access token is required")]
        public string AccessToken { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Access token secret is required")]
        public string AccessTokenSecret { get; set; }

        public bool HasDecryptionError { get; set; }
    }
}

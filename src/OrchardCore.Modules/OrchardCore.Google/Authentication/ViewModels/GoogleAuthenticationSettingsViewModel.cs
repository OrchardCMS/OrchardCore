using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Google.Authentication.ViewModels
{
    public class GoogleAuthenticationSettingsViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "ClientID key is required")]
        public string ClientID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "ClientSecret is required")]
        public string ClientSecret { get; set; }

        [RegularExpression(@"\/[-A-Za-z0-9+&@#\/%?=~_|!:,.;]+[-A-Za-z0-9+&@#\/%=~_|]", ErrorMessage = "Invalid path")]
        public string CallbackPath { get; set; }

        public bool SaveTokens { get; set; }

        public bool HasDecryptionError { get; set; }
    }
}

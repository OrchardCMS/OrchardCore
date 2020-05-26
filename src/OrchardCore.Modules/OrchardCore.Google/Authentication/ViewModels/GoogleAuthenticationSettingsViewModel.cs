using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Google.Authentication.ViewModels
{
    public class GoogleAuthenticationSettingsViewModel
    {
        //Error messages for empty fields
        [Required(AllowEmptyStrings = false, ErrorMessage = "ClientID key is required")]
        public string ClientID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "ClientSecret is required")]
        public string ClientSecret { get; set; }
        
        //Error message for wrong path and unacceptable symbols
        [RegularExpression(@"\/[-A-Za-z0-9+&@#\/%?=~_|!:,.;]+[-A-Za-z0-9+&@#\/%=~_|]", ErrorMessage = "Invalid path")]
        public string CallbackPath { get; set; }
    }
}

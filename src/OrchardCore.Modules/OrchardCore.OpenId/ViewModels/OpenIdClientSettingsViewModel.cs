using System.ComponentModel.DataAnnotations;

namespace OrchardCore.OpenId.ViewModels
{
    public class OpenIdClientSettingsViewModel
    {
        [Required]
        public string DisplayName { get; set; }

        public bool TestingModeEnabled { get; set; }

        [Required(ErrorMessage = "Authority is required")]
        public string Authority { get; set; }

        [Required(ErrorMessage = "ClientId is required")]
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        [RegularExpression(@"\/[-A-Za-z0-9+&@#\/%?=~_|!:,.;]+[-A-Za-z0-9+&@#\/%=~_|]", ErrorMessage = "Invalid path")]
        public string CallbackPath { get; set; }

        [Url(ErrorMessage = "Invalid SignedOut redirect url")]
        public string SignedOutRedirectUri { get; set; }

        [RegularExpression(@"\/[-A-Za-z0-9+&@#\/%?=~_|!:,.;]+[-A-Za-z0-9+&@#\/%=~_|]", ErrorMessage = "Invalid path")]
        public string SignedOutCallbackPath { get; set; }

        public string Scopes { get; set; }
        public string ResponseMode { get; set; }
        public bool StoreExternalTokens { get; set; }
        public bool UseCodeFlow { get; set; }
        public bool UseCodeIdTokenTokenFlow { get; set; }
        public bool UseCodeIdTokenFlow { get; set; }
        public bool UseCodeTokenFlow { get; set; }
        public bool UseIdTokenFlow { get; set; }
        public bool UseIdTokenTokenFlow { get; set; }
        public string Parameters { get; set; }
    }
}

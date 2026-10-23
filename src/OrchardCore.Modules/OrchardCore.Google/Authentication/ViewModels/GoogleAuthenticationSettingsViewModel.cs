using System.ComponentModel.DataAnnotations;
using OrchardCore.Secrets;

namespace OrchardCore.Google.Authentication.ViewModels;

public class GoogleAuthenticationSettingsViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "ClientID key is required")]
    public string ClientID { get; set; }

    public SecretInputViewModel ClientSecret { get; set; } = new();

    [RegularExpression(@"\/[-A-Za-z0-9+&@#\/%?=~_|!:,.;]+[-A-Za-z0-9+&@#\/%=~_|]", ErrorMessage = "Invalid path")]
    public string CallbackPath { get; set; }

    public bool SaveTokens { get; set; }
}

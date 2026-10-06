using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Google.Authentication.ViewModels;

public class GoogleAuthenticationSettingsViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "ClientID key is required")]
    public string ClientID { get; set; }

    public string ClientSecretSecretName { get; set; }

    [RegularExpression(@"\/[-A-Za-z0-9+&@#\/%?=~_|!:,.;]+[-A-Za-z0-9+&@#\/%=~_|]", ErrorMessage = "Invalid path")]
    public string CallbackPath { get; set; }

    public bool SaveTokens { get; set; }

    public bool HasClientSecret { get; set; }
}

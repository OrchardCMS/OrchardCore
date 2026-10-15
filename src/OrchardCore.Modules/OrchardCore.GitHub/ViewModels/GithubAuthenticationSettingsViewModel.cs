using System.ComponentModel.DataAnnotations;
using OrchardCore.Secrets;

namespace OrchardCore.GitHub.ViewModels;

public class GitHubAuthenticationSettingsViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "API key is required")]
    public string ClientID { get; set; }

    public SecretInputViewModel ClientSecret { get; set; } = new();

    [RegularExpression(@"\/[-A-Za-z0-9+&@#\/%?=~_|!:,.;]+[-A-Za-z0-9+&@#\/%=~_|]", ErrorMessage = "Invalid path")]
    public string CallbackUrl { get; set; }

    public bool SaveTokens { get; set; }
}

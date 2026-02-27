using System.ComponentModel.DataAnnotations;

namespace OrchardCore.GitHub.ViewModels;

public class GitHubAuthenticationSettingsViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "API key is required")]
    public string ClientID { get; set; }

    public string ClientSecretSecretName { get; set; }

    [RegularExpression(@"\/[-A-Za-z0-9+&@#\/%?=~_|!:,.;]+[-A-Za-z0-9+&@#\/%=~_|]", ErrorMessage = "Invalid path")]
    public string CallbackUrl { get; set; }

    public bool SaveTokens { get; set; }

    public bool HasClientSecret { get; set; }
}

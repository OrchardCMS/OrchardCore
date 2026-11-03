using System.ComponentModel.DataAnnotations;
using OrchardCore.Secrets;

namespace OrchardCore.Twitter.ViewModels;

public class TwitterSettingsViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "API key is required")]
    public string APIKey { get; set; }

    public SecretInputViewModel ConsumerSecret { get; set; } = new();

    [Required(AllowEmptyStrings = false, ErrorMessage = "Access token is required")]
    public string AccessToken { get; set; }

    public SecretInputViewModel AccessTokenSecret { get; set; } = new();
}

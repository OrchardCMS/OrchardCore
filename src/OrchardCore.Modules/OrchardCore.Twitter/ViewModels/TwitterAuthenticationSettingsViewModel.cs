using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Twitter.ViewModels;

public class TwitterAuthenticationSettingsViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Consumer key is required")]
    public string ConsumerKey { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Consumer secret key is required")]
    public string ConsumerSecret { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Access token is required")]
    public string AccessToken { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Access token secret is required")]
    public string AccessTokenSecret { get; set; }

    [RegularExpression(@"\/[-A-Za-z0-9+&@#\/%?=~_|!:,.;]+[-A-Za-z0-9+&@#\/%=~_|]", ErrorMessage = "Invalid path")]
    public string CallbackPath { get; set; }

    public bool SaveTokens { get; set; }

    public bool HasDecryptionError { get; set; }
}

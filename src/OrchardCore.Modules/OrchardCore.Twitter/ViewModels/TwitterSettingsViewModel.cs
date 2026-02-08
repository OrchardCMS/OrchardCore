using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Twitter.ViewModels;

public class TwitterSettingsViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "API key is required")]
    public string APIKey { get; set; }

    public string ConsumerSecretSecretName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Access token is required")]
    public string AccessToken { get; set; }

    public string AccessTokenSecretSecretName { get; set; }

    public bool HasConsumerSecret { get; set; }

    public bool HasAccessTokenSecret { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Facebook.ViewModels;

public class FacebookSettingsViewModel
{
    [Required]
    public string AppId { get; set; }

    public string AppSecretSecretName { get; set; }

    [Required]
    public string SdkJs { get; set; }

    public bool FBInit { get; set; }
    public string FBInitParams { get; set; }

    [RegularExpression(@"(v)\d+\.\d+")]
    public string Version { get; set; }

    public bool HasAppSecret { get; set; }
}

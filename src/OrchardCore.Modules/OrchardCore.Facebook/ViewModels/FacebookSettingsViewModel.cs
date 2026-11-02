using System.ComponentModel.DataAnnotations;
using OrchardCore.Secrets;

namespace OrchardCore.Facebook.ViewModels;

public class FacebookSettingsViewModel
{
    [Required]
    public string AppId { get; set; }

    public SecretInputViewModel AppSecret { get; set; } = new();

    [Required]
    public string SdkJs { get; set; }

    public bool FBInit { get; set; }
    public string FBInitParams { get; set; }

    [RegularExpression(@"(v)\d+\.\d+")]
    public string Version { get; set; }
}

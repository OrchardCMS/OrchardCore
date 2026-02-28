using System.ComponentModel.DataAnnotations;

namespace OrchardCore.ReCaptcha.ViewModels;

public class ReCaptchaSettingsViewModel
{
    [Required]
    public string SiteKey { get; set; }

    [Required]
    public string SecretKey { get; set; }
}

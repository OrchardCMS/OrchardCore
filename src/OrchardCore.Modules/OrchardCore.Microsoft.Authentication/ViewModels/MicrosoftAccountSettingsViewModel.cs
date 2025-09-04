using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Microsoft.Authentication.ViewModels;

public class MicrosoftAccountSettingsViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Application Id is required")]
    public string AppId { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Application Secret is required")]
    public string AppSecret { get; set; }

    [RegularExpression(@"\/[-A-Za-z0-9+&@#\/%?=~_|!:,.;]+[-A-Za-z0-9+&@#\/%=~_|]", ErrorMessage = "Invalid path")]
    public string CallbackPath { get; set; }

    public bool SaveTokens { get; set; }

    public bool HasDecryptionError { get; set; }
}

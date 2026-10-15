using System.ComponentModel.DataAnnotations;
using OrchardCore.Secrets;

namespace OrchardCore.Microsoft.Authentication.ViewModels;

public class MicrosoftAccountSettingsViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Application Id is required")]
    public string AppId { get; set; }

    public SecretInputViewModel AppSecret { get; set; } = new();

    [RegularExpression(@"\/[-A-Za-z0-9+&@#\/%?=~_|!:,.;]+[-A-Za-z0-9+&@#\/%=~_|]", ErrorMessage = "Invalid path")]
    public string CallbackPath { get; set; }

    public bool SaveTokens { get; set; }
}

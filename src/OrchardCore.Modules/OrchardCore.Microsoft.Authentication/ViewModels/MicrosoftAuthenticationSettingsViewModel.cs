using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Microsoft.Authentication.ViewModels
{
    public class MicrosoftAuthenticationSettingsViewModel
    {
        [Required]
        public string AppId { get; set; }

        [Required]
        public string AppSecret { get; set; }

        [RegularExpression(@"\/[-A-Za-z0-9+&@#\/%?=~_|!:,.;]+[-A-Za-z0-9+&@#\/%=~_|]", ErrorMessage = "Invalid path")]
        public string CallbackPath { get; set; }
    }
}

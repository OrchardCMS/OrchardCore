using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Facebook.ViewModels
{
    public class FacebookSettingsViewModel
    {
        [Required]
        public string AppId { get; set; }

        [Required]
        public string AppSecret { get; set; }

        [Required]
        public string SdkJs { get; set; }

        public bool FBInit { get; set; }
        public string FBInitParams { get; set; }

        [RegularExpression(@"(v)\d+\.\d+")]
        public string Version { get; set; }

        public bool HasDecryptionError { get; set; }
    }
}

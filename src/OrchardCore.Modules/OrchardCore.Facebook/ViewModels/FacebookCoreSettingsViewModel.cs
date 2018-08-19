using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Facebook.ViewModels
{
    public class FacebookCoreSettingsViewModel
    {
        [Required]
        public string AppId { get; set; }
        public string AppSecret { get; set; }
    }
}

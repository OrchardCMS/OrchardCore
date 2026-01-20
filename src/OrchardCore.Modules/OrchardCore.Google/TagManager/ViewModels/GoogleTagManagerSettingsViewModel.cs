using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Google.TagManager.ViewModels
{
    public class GoogleTagManagerSettingsViewModel
    {
        [Required(AllowEmptyStrings = false)]
        public string ContainerID { get; set; }
    }
}

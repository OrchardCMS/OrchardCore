using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.ViewModels
{
    public class SaveFormAttachmentsTaskViewModel
    {
        [Required]
        public string Folder { get; set; }

        [Required]
        public bool UseMediaFileStore { get; set; }
    }
}

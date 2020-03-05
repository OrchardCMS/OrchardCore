using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Twitter.Workflows.ViewModels
{
    public class UpdateTwitterStatusTaskViewModel
    {
        [Required]
        public string StatusTemplate { get; set; }
    }
}

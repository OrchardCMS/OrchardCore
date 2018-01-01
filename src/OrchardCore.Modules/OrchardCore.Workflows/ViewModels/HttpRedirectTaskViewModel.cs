using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.ViewModels
{
    public class HttpRedirectTaskViewModel
    {
        [Required]
        public string Location { get; set; }
        public string Permanent { get; set; }
    }
}

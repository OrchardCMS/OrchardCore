using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.Http.ViewModels
{
    public class HttpRedirectTaskViewModel
    {
        [Required]
        public string Location { get; set; }

        public bool Permanent { get; set; }
    }
}

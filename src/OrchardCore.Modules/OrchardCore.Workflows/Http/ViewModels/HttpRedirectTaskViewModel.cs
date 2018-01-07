using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.Http.ViewModels
{
    public class HttpRedirectTaskViewModel
    {
        [Required]
        public string Location { get; set; }
        public string Permanent { get; set; }
    }
}

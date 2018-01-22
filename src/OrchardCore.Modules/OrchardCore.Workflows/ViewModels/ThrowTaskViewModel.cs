using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.ViewModels
{
    public class ThrowTaskViewModel
    {
        [Required]
        public string ExceptionType { get; set; }

        public string Message { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.ViewModels
{
    public class TryCatchTaskViewModel
    {
        [Required]
        public string ExceptionTypes { get; set; }
    }
}

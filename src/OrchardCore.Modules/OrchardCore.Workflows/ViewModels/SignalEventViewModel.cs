using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.ViewModels
{
    public class SignalEventViewModel
    {
        [Required]
        public string SignalName { get; set; }
    }
}

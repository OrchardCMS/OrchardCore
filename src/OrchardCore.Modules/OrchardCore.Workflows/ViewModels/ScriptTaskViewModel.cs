using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.ViewModels
{
    public class ScriptTaskViewModel
    {
        [Required]
        public string AvailableOutcomes { get; set; }

        [Required]
        public string Script { get; set; }
    }
}

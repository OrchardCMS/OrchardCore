using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.ViewModels
{
    public class ScriptTaskViewModel
    {
        public string Title { get; set; }

        [Required]
        public string AvailableOutcomes { get; set; }

        [Required]
        public string Script { get; set; }
    }
}

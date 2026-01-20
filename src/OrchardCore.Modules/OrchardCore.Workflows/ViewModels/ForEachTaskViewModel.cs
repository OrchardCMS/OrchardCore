using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.ViewModels
{
    public class ForEachTaskViewModel
    {
        [Required]
        public string EnumerableExpression { get; set; }

        [Required]
        public string LoopVariableName { get; set; }
    }
}

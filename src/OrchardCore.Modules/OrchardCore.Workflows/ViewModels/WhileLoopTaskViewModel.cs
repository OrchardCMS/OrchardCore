using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.ViewModels
{
    public class WhileLoopTaskViewModel
    {
        [Required]
        public string ConditionExpression { get; set; }
    }
}

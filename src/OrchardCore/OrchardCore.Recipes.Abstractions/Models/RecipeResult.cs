using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Recipes.Models
{
    public class RecipeResult
    {
        public string ExecutionId { get; set; }
        public List<RecipeStepResult> Steps { get; set; } = new List<RecipeStepResult>();
        public bool IsCompleted => Steps.All(s => s.IsCompleted);
        public bool IsSuccessful => Steps.All(s => s.IsCompleted && s.IsSuccessful);
    }
}

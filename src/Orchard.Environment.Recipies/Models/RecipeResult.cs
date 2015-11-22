using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Recipes.Models
{
    public class RecipeResult
    {
        public string ExecutionId { get; set; }
        public IEnumerable<RecipeStepResult> Steps { get; set; }
        public bool IsCompleted
        {
            get
            {
                return Steps.All(s => s.IsCompleted);
            }
        }
        public bool IsSuccessful
        {
            get
            {
                return IsCompleted && Steps.All(s => s.IsSuccessful);
            }
        }
    }
}
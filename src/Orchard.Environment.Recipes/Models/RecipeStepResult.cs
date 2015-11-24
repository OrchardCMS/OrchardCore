using System.Xml.Linq;

namespace Orchard.Environment.Recipes.Models
{
    public class RecipeStepResult
    {
        public string RecipeName { get; set; }
        public string StepName { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
    }
}
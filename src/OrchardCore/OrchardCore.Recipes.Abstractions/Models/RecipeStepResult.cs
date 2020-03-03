namespace OrchardCore.Recipes.Models
{
    public class RecipeStepResult
    {
        public string StepName { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
    }
}

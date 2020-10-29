namespace OrchardCore.Recipes.ViewModels
{
    public class RecipeViewModel
    {
        public string FileName { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string[] Tags { get; set; }
        public bool IsSetupRecipe { get; set; }
        public string Description { get; set; }
        public string BasePath { get; set; }
        public string Feature { get; set; }
    }
}

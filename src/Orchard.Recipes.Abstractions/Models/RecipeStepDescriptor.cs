namespace Orchard.Recipes.Models
{
    public class RecipeStepDescriptor
    {
        public RecipeStepDescriptor(string id, string recipeName, string name)
        {
            Id = id;
            RecipeName = recipeName;
            Name = name;
        }

        public string Id { get; set; }
        public string RecipeName { get; private set; }
        public string Name { get; private set; }
    }
}
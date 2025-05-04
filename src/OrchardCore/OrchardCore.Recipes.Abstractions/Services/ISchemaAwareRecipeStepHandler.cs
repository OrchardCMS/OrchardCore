namespace OrchardCore.Recipes.Services;

public interface ISchemaAwareRecipeStepHandler : IRecipeStepHandler
{
    RecipeSchema GetSchema();
}

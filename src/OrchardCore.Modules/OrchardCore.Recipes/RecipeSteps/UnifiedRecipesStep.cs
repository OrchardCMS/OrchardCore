using Json.Schema;
using Microsoft.Extensions.Localization;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes.RecipeSteps;

/// <summary>
/// Unified recipe step for executing nested recipes.
/// </summary>
public sealed class UnifiedRecipesStep : RecipeImportStep<UnifiedRecipesStep.RecipesStepModel>
{
    private readonly IEnumerable<IRecipeHarvester> _recipeHarvesters;

    internal readonly IStringLocalizer S;

    public UnifiedRecipesStep(
        IEnumerable<IRecipeHarvester> recipeHarvesters,
        IStringLocalizer<UnifiedRecipesStep> stringLocalizer)
    {
        _recipeHarvesters = recipeHarvesters;
        S = stringLocalizer;
    }

    /// <inheritdoc />
    public override string Name => "Recipes";

    /// <inheritdoc />
    public override string DisplayName => "Recipes";

    /// <inheritdoc />
    public override string Description => "Executes nested recipes by name.";

    /// <inheritdoc />
    public override string Category => "Configuration";

    /// <inheritdoc />
    protected override JsonSchema BuildSchema()
    {
        return new JsonSchemaBuilder()
            .Schema(MetaSchemas.Draft202012Id)
            .Type(SchemaValueType.Object)
            .Title(Name)
            .Description(Description)
            .Required("name", "Values")
            .Properties(
                ("name", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("Values", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Array)
                    .Description("Array of recipes to execute.")
                    .Items(new JsonSchemaBuilder()
                        .Type(SchemaValueType.Object)
                        .Required("name")
                        .Properties(
                            ("executionid", new JsonSchemaBuilder()
                                .Type(SchemaValueType.String)
                                .Description("Optional execution ID for this recipe.")),
                            ("name", new JsonSchemaBuilder()
                                .Type(SchemaValueType.String)
                                .Description("The name of the recipe to execute."))))))
            .AdditionalProperties(false)
            .Build();
    }

    /// <inheritdoc />
    protected override async Task ImportAsync(RecipesStepModel model, RecipeExecutionContext context)
    {
        var recipeCollections = await Task.WhenAll(_recipeHarvesters.Select(harvester => harvester.HarvestRecipesAsync()));
        var recipes = recipeCollections.SelectMany(recipe => recipe).ToDictionary(recipe => recipe.Name);

        var innerRecipes = new List<IRecipeDescriptor>();
        foreach (var recipe in model.Values ?? [])
        {
            if (!recipes.TryGetValue(recipe.Name, out var value))
            {
                context.Errors.Add(S["No recipe named '{0}' was found.", recipe.Name]);
                continue;
            }

            innerRecipes.Add(value);
        }

        context.InnerRecipes = innerRecipes;
    }

    /// <summary>
    /// Model for the Recipes step data.
    /// </summary>
    public sealed class RecipesStepModel
    {
        public RecipeReference[] Values { get; set; }
    }

    /// <summary>
    /// Reference to a recipe to execute.
    /// </summary>
    public sealed class RecipeReference
    {
        public string ExecutionId { get; set; }

        public string Name { get; set; }
    }
}

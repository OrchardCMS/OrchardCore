using OrchardCore.Recipes.Schema;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.Placements.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Placements.Recipes;

public sealed class PlacementRecipeStep : RecipeDeploymentStep<PlacementRecipeStep.PlacementsStepModel>
{
    private readonly PlacementsManager _placementsManager;

    public PlacementRecipeStep(PlacementsManager placementsManager)
    {
        _placementsManager = placementsManager;
    }

    public override string Name => "Placements";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Placements")
            .Description("Updates display/editor placement rules.")
            .Required("name", "Placements")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("Placements", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .AdditionalProperties(true)
                    .Description("A dictionary keyed by shape type. Each value is an array of placement objects.")))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(PlacementsStepModel model, RecipeExecutionContext context)
    {
        if (model.Placements != null)
        {
            foreach (var placement in model.Placements)
            {
                await _placementsManager.UpdateShapePlacementsAsync(placement.Key, placement.Value);
            }
        }
    }

    protected override async Task<PlacementsStepModel> BuildExportModelAsync(RecipeExportContext context)
    {
        var placements = await _placementsManager.ListShapePlacementsAsync();

        return new PlacementsStepModel
        {
            Placements = placements.ToDictionary(k => k.Key, v => v.Value),
        };
    }

    public sealed class PlacementsStepModel
    {
        public Dictionary<string, PlacementNode[]> Placements { get; set; }
    }
}

using System.Text.Json.Nodes;
using OrchardCore.Recipes.Schema;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.Placements.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Placements.Recipes;

public sealed class PlacementRecipeStep : RecipeImportStep<object>
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

    protected override async Task ImportAsync(object model, RecipeExecutionContext context)
    {
        if (context.Step.TryGetPropertyValue("Placements", out var jsonNode) && jsonNode is JsonObject templates)
        {
            foreach (var property in templates)
            {
                var name = property.Key;
                var value = property.Value.ToObject<PlacementNode[]>();

                await _placementsManager.UpdateShapePlacementsAsync(name, value);
            }
        }
    }
}

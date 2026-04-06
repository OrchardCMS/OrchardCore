using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Schema;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Features.Recipes;

/// <summary>
/// Unified recipe/deployment step for enabling and disabling features.
/// </summary>
/// <remarks>
/// <para>
/// This is an example of the unified <see cref="IRecipeDeploymentStep"/> approach where a single class handles:
/// </para>
/// <list type="bullet">
///   <item>JSON Schema definition for validation and IDE support</item>
///   <item>Recipe import (enabling/disabling features)</item>
///   <item>Deployment export (exporting current feature state)</item>
/// </list>
/// <para>
/// <b>Comparison to existing approach:</b>
/// </para>
/// <list type="bullet">
///   <item>Previously: <c>FeatureStep</c> (handler) + <c>FeatureStepDescriptor</c> (schema) + deployment source</item>
///   <item>Now: Single <c>UnifiedFeatureStep</c> class handles everything</item>
/// </list>
/// </remarks>
public sealed class UnifiedFeatureStep : RecipeDeploymentStep<UnifiedFeatureStep.FeatureStepModel>
{
    private readonly IShellFeaturesManager _shellFeaturesManager;

    public UnifiedFeatureStep(IShellFeaturesManager shellFeaturesManager)
    {
        _shellFeaturesManager = shellFeaturesManager;
    }

    /// <inheritdoc />
    public override string Name => "Feature";

    /// <inheritdoc />
    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title(Name)
            .Description("Enables or disables features in the Orchard Core application.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("enable", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Description("Array of feature IDs to enable.")
                    .Items(new RecipeStepSchemaBuilder().TypeString())),
                ("disable", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Description("Array of feature IDs to disable.")
                    .Items(new RecipeStepSchemaBuilder().TypeString())))
            .AdditionalProperties(false)
            .Build();
    }

    /// <inheritdoc />
    protected override async Task ImportAsync(FeatureStepModel model, RecipeExecutionContext context)
    {
        var features = await _shellFeaturesManager.GetAvailableFeaturesAsync();

        var featuresToDisable = features.Where(x => model.Disable?.Contains(x.Id) == true).ToArray();
        var featuresToEnable = features.Where(x => model.Enable?.Contains(x.Id) == true).ToArray();

        if (featuresToDisable.Length > 0 || featuresToEnable.Length > 0)
        {
            await _shellFeaturesManager.UpdateFeaturesAsync(featuresToDisable, featuresToEnable, true);
        }
    }

    /// <inheritdoc />
    protected override async Task<FeatureStepModel> BuildExportModelAsync(RecipeExportContext context)
    {
        var enabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();

        return new FeatureStepModel
        {
            Enable = enabledFeatures.Select(f => f.Id).OrderBy(id => id).ToArray(),
        };
    }

    /// <summary>
    /// Model for the Feature step data.
    /// </summary>
    public sealed class FeatureStepModel
    {
        /// <summary>
        /// Gets or sets the step name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the array of feature IDs to enable.
        /// </summary>
        public string[] Enable { get; set; }

        /// <summary>
        /// Gets or sets the array of feature IDs to disable.
        /// </summary>
        public string[] Disable { get; set; }
    }
}

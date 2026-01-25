using System.Text.Json.Nodes;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Context for recipe/deployment export operations.
/// </summary>
public sealed class RecipeExportContext
{
    /// <summary>
    /// Gets or sets the service provider for resolving dependencies.
    /// </summary>
    public IServiceProvider ServiceProvider { get; init; }

    /// <summary>
    /// Gets or sets the list of step data objects being built for export.
    /// </summary>
    public IList<JsonObject> Steps { get; init; } = [];

    /// <summary>
    /// Gets or sets additional data shared during the export process.
    /// </summary>
    public IDictionary<string, object> Properties { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Adds a step to the export output.
    /// </summary>
    /// <param name="stepData">The step data to add.</param>
    public void AddStep(JsonObject stepData)
    {
        ArgumentNullException.ThrowIfNull(stepData);
        Steps.Add(stepData);
    }
}

using System.Text.Json.Nodes;
using OrchardCore.DataOrchestrator.Models;

namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Represents an activity in an ETL pipeline.
/// </summary>
public interface IEtlActivity
{
    /// <summary>
    /// Gets the technical name of this activity.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the display text for this activity.
    /// </summary>
    string DisplayText { get; }

    /// <summary>
    /// Gets the category of this activity: "Sources", "Transforms", or "Loads".
    /// </summary>
    string Category { get; }

    /// <summary>
    /// Gets or sets the activity properties as a JSON object.
    /// </summary>
    JsonObject Properties { get; set; }

    /// <summary>
    /// Gets whether this activity has an editor.
    /// </summary>
    bool HasEditor { get; }

    /// <summary>
    /// Returns the possible outcomes of this activity.
    /// </summary>
    IEnumerable<EtlOutcome> GetPossibleOutcomes();

    /// <summary>
    /// Executes the activity.
    /// </summary>
    Task<EtlActivityResult> ExecuteAsync(EtlExecutionContext context);
}

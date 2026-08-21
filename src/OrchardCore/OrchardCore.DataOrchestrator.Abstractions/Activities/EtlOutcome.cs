namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Represents a possible outcome of an ETL activity.
/// </summary>
public sealed class EtlOutcome
{
    public EtlOutcome(string name)
        : this(name, name)
    {
    }

    public EtlOutcome(string name, string displayName)
    {
        Name = name;
        DisplayName = displayName;
    }

    /// <summary>
    /// Gets the technical name of the outcome.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the display name of the outcome.
    /// </summary>
    public string DisplayName { get; }
}

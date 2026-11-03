using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The settings of an <see cref="AggregateStep"/>.
/// </summary>
public sealed class AggregateStepSettings
{
    /// <summary>
    /// Gets or sets the fields whose values define the groups. When empty, every row belongs to one group.
    /// </summary>
    public List<string> GroupBy { get; set; } = [];

    /// <summary>
    /// Gets or sets the values computed for each group.
    /// </summary>
    public List<AggregateMeasure> Measures { get; set; } = [];
}

/// <summary>
/// A value computed for each group.
/// </summary>
public sealed class AggregateMeasure
{
    /// <summary>
    /// Gets or sets the name of the field that holds the value.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the function to compute.
    /// </summary>
    public DataAggregate Function { get; set; } = DataAggregate.Count;

    /// <summary>
    /// Gets or sets the field the function reads. A count without a field counts the rows.
    /// </summary>
    public string Field { get; set; }
}

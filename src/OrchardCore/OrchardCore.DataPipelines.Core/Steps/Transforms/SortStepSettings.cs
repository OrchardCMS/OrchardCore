namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The settings of a <see cref="SortStep"/>.
/// </summary>
public sealed class SortStepSettings
{
    /// <summary>
    /// Gets or sets the fields to sort by, the most significant first.
    /// </summary>
    public List<SortField> Keys { get; set; } = [];
}

/// <summary>
/// A field to sort by.
/// </summary>
public sealed class SortField
{
    /// <summary>
    /// Gets or sets the name of the field.
    /// </summary>
    public string Field { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the largest values come first.
    /// </summary>
    public bool Descending { get; set; }
}

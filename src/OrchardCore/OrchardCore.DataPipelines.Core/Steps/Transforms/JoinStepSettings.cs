using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The settings of a <see cref="JoinStep"/>.
/// </summary>
public sealed class JoinStepSettings
{
    /// <summary>
    /// Gets or sets which rows without a match are kept.
    /// </summary>
    public DataJoinType JoinType { get; set; } = DataJoinType.Inner;

    /// <summary>
    /// Gets or sets the pairs of fields whose values must be equal for two rows to match.
    /// </summary>
    public List<JoinKey> Keys { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether text keys match ignoring case.
    /// </summary>
    public bool IgnoreCase { get; set; }

    /// <summary>
    /// Gets or sets the prefix added to a field of the right input whose name the left input already has. Defaults
    /// to <c>Right.</c>.
    /// </summary>
    public string RightPrefix { get; set; } = "Right.";
}

/// <summary>
/// A pair of fields that must hold equal values for two rows to match.
/// </summary>
public sealed class JoinKey
{
    /// <summary>
    /// Gets or sets the field of the left input.
    /// </summary>
    public string LeftField { get; set; }

    /// <summary>
    /// Gets or sets the field of the right input.
    /// </summary>
    public string RightField { get; set; }
}

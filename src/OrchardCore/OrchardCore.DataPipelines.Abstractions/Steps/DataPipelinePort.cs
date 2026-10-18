namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// An input or output of a step.
/// </summary>
public sealed class DataPipelinePort
{
    /// <summary>
    /// The name of the single input of most steps.
    /// </summary>
    public const string Input = "Input";

    /// <summary>
    /// The name of the single output of most steps.
    /// </summary>
    public const string Output = "Output";

    /// <summary>
    /// Initializes a new instance of the <see cref="DataPipelinePort"/> class.
    /// </summary>
    /// <param name="name">The stable technical name of the port.</param>
    /// <param name="displayName">The label shown on the designer.</param>
    /// <param name="kind">What flows through the port.</param>
    public DataPipelinePort(string name, string displayName, DataPipelinePortKind kind = DataPipelinePortKind.Records)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        Name = name;
        DisplayName = displayName ?? name;
        Kind = kind;
    }

    /// <summary>
    /// Gets the stable technical name of the port. Connections store this name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the label shown on the designer.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets what flows through the port.
    /// </summary>
    public DataPipelinePortKind Kind { get; }

    /// <summary>
    /// Gets or sets a value indicating whether an input must be connected for the pipeline to run.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether an input accepts several connections, whose data it reads one after
    /// the other, in the order of the connections.
    /// </summary>
    public bool AllowsMany { get; init; }
}

namespace OrchardCore.RemoteManagement;

/// <summary>
/// Describes arguments added to a suggested CLI command when a response property is empty.
/// </summary>
public sealed class CliNextStepOptionalArguments
{
    /// <summary>
    /// Initializes a new instance of <see cref="CliNextStepOptionalArguments"/>.
    /// </summary>
    /// <param name="whenMissing">The response property that must be empty for the arguments to be added.</param>
    /// <param name="arguments">The arguments to add.</param>
    public CliNextStepOptionalArguments(string whenMissing, params IEnumerable<string> arguments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(whenMissing);
        ArgumentNullException.ThrowIfNull(arguments);

        WhenMissing = whenMissing;
        Arguments = [.. arguments];
    }

    /// <summary>
    /// Gets the response property that must be empty for the arguments to be added.
    /// </summary>
    public string WhenMissing { get; }

    /// <summary>
    /// Gets the arguments to add.
    /// </summary>
    public IReadOnlyList<string> Arguments { get; }
}

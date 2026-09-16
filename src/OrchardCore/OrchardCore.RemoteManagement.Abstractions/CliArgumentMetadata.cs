namespace OrchardCore.RemoteManagement;

/// <summary>
/// Describes an OpenAPI parameter that is exposed as a positional CLI argument.
/// </summary>
public sealed class CliArgumentMetadata
{
    /// <summary>
    /// Initializes a new instance of <see cref="CliArgumentMetadata"/>.
    /// </summary>
    /// <param name="parameterName">The OpenAPI parameter name.</param>
    /// <param name="position">The zero-based argument position.</param>
    public CliArgumentMetadata(string parameterName, int position)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);
        ArgumentOutOfRangeException.ThrowIfNegative(position);

        ParameterName = parameterName;
        Position = position;
    }

    /// <summary>
    /// Gets the OpenAPI parameter name.
    /// </summary>
    public string ParameterName { get; }

    /// <summary>
    /// Gets the zero-based argument position.
    /// </summary>
    public int Position { get; }
}

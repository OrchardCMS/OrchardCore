namespace OrchardCore.RemoteManagement;

/// <summary>
/// Describes a property displayed by the optional CLI table output.
/// </summary>
public sealed class CliTableColumnMetadata
{
    /// <summary>
    /// Initializes a new instance of <see cref="CliTableColumnMetadata"/>.
    /// </summary>
    /// <param name="propertyPath">The JSON property path.</param>
    /// <param name="heading">The table column heading.</param>
    public CliTableColumnMetadata(string propertyPath, string heading)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(heading);

        PropertyPath = propertyPath;
        Heading = heading;
    }

    /// <summary>
    /// Gets the JSON property path.
    /// </summary>
    public string PropertyPath { get; }

    /// <summary>
    /// Gets the table column heading.
    /// </summary>
    public string Heading { get; }
}

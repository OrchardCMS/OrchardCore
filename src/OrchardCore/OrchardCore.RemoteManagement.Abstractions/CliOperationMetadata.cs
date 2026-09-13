namespace OrchardCore.RemoteManagement;

/// <summary>
/// Describes how an OpenAPI operation is projected into the dynamic CLI command tree.
/// </summary>
public sealed class CliOperationMetadata
{
    /// <summary>
    /// Initializes a new instance of <see cref="CliOperationMetadata"/>.
    /// </summary>
    /// <param name="commandGroup">The noun-based command group segments.</param>
    /// <param name="verb">The command verb.</param>
    public CliOperationMetadata(IEnumerable<string> commandGroup, string verb)
    {
        ArgumentNullException.ThrowIfNull(commandGroup);
        ArgumentException.ThrowIfNullOrWhiteSpace(verb);

        var segments = commandGroup.ToArray();

        if (segments.Length == 0 || segments.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("At least one non-empty command group segment is required.", nameof(commandGroup));
        }

        CommandGroup = segments;
        Verb = verb;
    }

    /// <summary>
    /// Gets the noun-based command group segments.
    /// </summary>
    public IReadOnlyList<string> CommandGroup { get; }

    /// <summary>
    /// Gets the command verb.
    /// </summary>
    public string Verb { get; }

    /// <summary>
    /// Gets or sets the capability that owns the operation.
    /// </summary>
    public string Capability { get; set; }

    /// <summary>
    /// Gets or initializes alternative command verbs retained for compatibility.
    /// </summary>
    public IList<string> Aliases { get; init; } = [];

    /// <summary>
    /// Gets or initializes positional argument mappings.
    /// </summary>
    public IList<CliArgumentMetadata> Arguments { get; init; } = [];

    /// <summary>
    /// Gets or initializes columns used by the optional table output.
    /// </summary>
    public IList<CliTableColumnMetadata> TableColumns { get; init; } = [];

    /// <summary>
    /// Gets or initializes request body properties whose values must be collected through secret-safe CLI inputs.
    /// </summary>
    public IList<string> SecretProperties { get; init; } = [];

    /// <summary>
    /// Gets or sets whether the JSON response contains one-time secrets and must be saved through private CLI output.
    /// </summary>
    public bool SecretResponse { get; set; }

    /// <summary>Gets or sets whether the response must be streamed to an explicit new file.</summary>
    public bool FileResponse { get; set; }

    /// <summary>
    /// Gets or sets how the request body is supplied.
    /// </summary>
    public CliInputMode InputMode { get; set; } = CliInputMode.Options;

    /// <summary>
    /// Gets or sets the default JSON request body used when the caller does not supply one.
    /// </summary>
    public string DefaultJsonBody { get; set; }

    /// <summary>
    /// Gets or sets whether interactive callers must confirm the operation.
    /// </summary>
    public bool RequiresConfirmation { get; set; }

    /// <summary>
    /// Gets or sets whether the command is hidden from normal help output.
    /// </summary>
    public bool Hidden { get; set; }
}

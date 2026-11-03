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
    /// Gets or sets the singular name of the resource, which CLI summaries use, as in "Tenant 'Demo' created successfully.".
    /// </summary>
    public string ResourceName { get; set; }

    /// <summary>
    /// Gets or sets the phrase that CLI summaries use for a verb they can't put in the past tense, as in
    /// "Tenant 'Demo' reloaded successfully.". This requires <see cref="ResourceName"/>.
    /// </summary>
    public string CompletedAction { get; set; }

    /// <summary>
    /// Gets or initializes the commands that human-readable output suggests after the operation succeeds.
    /// </summary>
    public IList<CliNextStepMetadata> NextSteps { get; init; } = [];

    /// <summary>
    /// Gets or initializes the response properties that human-readable output shows after the operation changes
    /// something. All of them are shown when this is empty.
    /// </summary>
    public IList<string> SummaryProperties { get; init; } = [];

    /// <summary>
    /// Gets or sets the response property holding the rows of a list, for table output without <see cref="TableColumns"/>.
    /// </summary>
    public string ItemsProperty { get; set; }

    /// <summary>
    /// Gets or sets the request body property holding the password of a new administrator. The CLI requires it, and
    /// applies the setup password rules before sending the request.
    /// </summary>
    public string SetupPasswordProperty { get; set; }

    /// <summary>
    /// Gets or sets whether the JSON response contains one-time secrets and must be saved through private CLI output.
    /// </summary>
    public bool SecretResponse { get; set; }

    /// <summary>
    /// Gets or sets the boolean parameter or body property that makes the operation create an administrative
    /// application. Pomi offers it as an option, and saves the returned credentials in a new context.
    /// </summary>
    public string ProvisioningProperty { get; set; }

    /// <summary>
    /// Gets or sets whether the operation is left out of MCP tools. Operations with secret properties or a
    /// secret response always are; set this for one that can return credentials depending on its input.
    /// </summary>
    public bool ExcludedFromMcp { get; set; }

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
    /// Gets or sets whether the operation does its work before it responds, which can take minutes. The CLI waits
    /// longer for the response of such an operation.
    /// </summary>
    public bool LongRunning { get; set; }

    /// <summary>
    /// Gets or sets whether interactive callers must confirm the operation. Such an operation is
    /// left out of MCP tools, unless the tenant allows them with the MCP tool options.
    /// </summary>
    public bool RequiresConfirmation { get; set; }

    /// <summary>
    /// Gets or sets whether the command is hidden from normal help output.
    /// </summary>
    public bool Hidden { get; set; }
}

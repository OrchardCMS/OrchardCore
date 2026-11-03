namespace OrchardCore.RemoteManagement;

/// <summary>
/// Describes a command that human-readable CLI output suggests after an operation succeeds. The first step of an
/// operation whose condition matches the response is shown.
/// </summary>
public sealed class CliNextStepMetadata
{
    /// <summary>
    /// Gets or sets the sentence introducing the command, as in "start this tenant.".
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the response property that <see cref="WhenValue"/> is compared to. The step always applies when this is empty.
    /// </summary>
    public string WhenProperty { get; set; }

    /// <summary>
    /// Gets or sets the value that <see cref="WhenProperty"/> must have for the step to apply.
    /// </summary>
    public string WhenValue { get; set; }

    /// <summary>
    /// Gets or initializes the arguments of the suggested command, after the CLI executable name. An argument written
    /// as <c>{property}</c> is replaced by that response property, and nothing is suggested if it has no safe value.
    /// </summary>
    public IList<string> Arguments { get; init; } = [];

    /// <summary>
    /// Gets or initializes the arguments added to the suggested command when a response property is empty.
    /// </summary>
    public IList<CliNextStepOptionalArguments> OptionalArguments { get; init; } = [];

    /// <summary>
    /// Gets or sets the text shown after the suggested command.
    /// </summary>
    public string Note { get; set; }

    /// <summary>
    /// Gets or sets whether the step suggests saving a CLI context for the tenant that the response describes through its
    /// <c>name</c> and <c>url</c> or <c>primaryUrl</c> properties, rather than <see cref="Arguments"/>.
    /// </summary>
    public bool SavesContext { get; set; }
}

namespace OrchardCore.RemoteManagement;

/// <summary>
/// Defines how a CLI command accepts an OpenAPI request body.
/// </summary>
public enum CliInputMode
{
    /// <summary>
    /// Maps scalar request properties to command options.
    /// </summary>
    Options,

    /// <summary>
    /// Accepts a JSON value directly, from a file, or from standard input.
    /// </summary>
    Json,

    /// <summary>
    /// Accepts a binary stream from a file or standard input.
    /// </summary>
    Stream,
}

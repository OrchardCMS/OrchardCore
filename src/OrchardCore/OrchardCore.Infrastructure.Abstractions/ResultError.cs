using Microsoft.Extensions.Localization;

namespace OrchardCore.Infrastructure;

/// <summary>
/// Represents an error that occurred during an operation.
/// </summary>
public class ResultError
{
    /// <summary>
    /// Gets or sets the key associated with the error.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message associated with the error.
    /// </summary>
    public LocalizedString Message { get; set; }
}

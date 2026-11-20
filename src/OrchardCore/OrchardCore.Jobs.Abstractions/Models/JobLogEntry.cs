using Microsoft.Extensions.Logging;

namespace OrchardCore.Jobs.Models;

/// <summary>
/// A message recorded during a run.
/// </summary>
public sealed class JobLogEntry
{
    /// <summary>
    /// Gets or sets when the message was recorded, in UTC.
    /// </summary>
    public DateTime Utc { get; set; }

    /// <summary>
    /// Gets or sets the severity of the message.
    /// </summary>
    public LogLevel Level { get; set; }

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    public string Message { get; set; }
}

using Microsoft.Extensions.Logging;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// A message recorded during a run, shown in its history.
/// </summary>
public sealed class DataPipelineLogEntry
{
    /// <summary>
    /// Gets or sets when the message was recorded, in UTC.
    /// </summary>
    public DateTime Utc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the level of the message: <see cref="LogLevel.Information"/>, <see cref="LogLevel.Warning"/> or
    /// <see cref="LogLevel.Error"/>.
    /// </summary>
    public LogLevel Level { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets or sets the step the message is about, or <see langword="null"/> for the whole run.
    /// </summary>
    public string StepId { get; set; }

    /// <summary>
    /// Gets or sets the message. It must not contain secrets.
    /// </summary>
    public string Message { get; set; }
}

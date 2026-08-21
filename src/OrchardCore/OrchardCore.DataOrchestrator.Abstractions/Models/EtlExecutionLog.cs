namespace OrchardCore.DataOrchestrator.Models;

/// <summary>
/// Represents a log entry for an ETL pipeline execution.
/// </summary>
public sealed class EtlExecutionLog
{
    /// <summary>
    /// Gets or sets the document identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the pipeline identifier.
    /// </summary>
    public string PipelineId { get; set; }

    /// <summary>
    /// Gets or sets the pipeline name at the time of execution.
    /// </summary>
    public string PipelineName { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when execution started.
    /// </summary>
    public DateTime StartedUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when execution completed.
    /// </summary>
    public DateTime? CompletedUtc { get; set; }

    /// <summary>
    /// Gets or sets the execution status: "Running", "Success", "Failed", or "Cancelled".
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Gets or sets the number of records processed.
    /// </summary>
    public int RecordsProcessed { get; set; }

    /// <summary>
    /// Gets or sets the number of records successfully loaded.
    /// </summary>
    public int RecordsLoaded { get; set; }

    /// <summary>
    /// Gets or sets the number of errors encountered.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Gets or sets the error messages from the execution.
    /// </summary>
    public IList<string> Errors { get; set; } = [];

    /// <summary>
    /// Gets or sets the parameter values used for the execution.
    /// </summary>
    public IDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
}

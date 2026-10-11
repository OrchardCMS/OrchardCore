namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Counts what a step read and wrote during a run. The engine counts rows and files as they flow through the step's
/// inputs and outputs; the counters are safe to read while the step runs.
/// </summary>
public sealed class DataPipelineStepMetrics
{
    private long _rowsIn;
    private long _rowsOut;
    private long _filesIn;
    private long _filesOut;
    private long _warnings;

    /// <summary>
    /// Gets the number of rows the step read.
    /// </summary>
    public long RowsIn => Interlocked.Read(ref _rowsIn);

    /// <summary>
    /// Gets the number of rows the step wrote, to all its outputs.
    /// </summary>
    public long RowsOut => Interlocked.Read(ref _rowsOut);

    /// <summary>
    /// Gets the number of files the step read.
    /// </summary>
    public long FilesIn => Interlocked.Read(ref _filesIn);

    /// <summary>
    /// Gets the number of files the step wrote.
    /// </summary>
    public long FilesOut => Interlocked.Read(ref _filesOut);

    /// <summary>
    /// Gets the number of warnings the step reported.
    /// </summary>
    public long Warnings => Interlocked.Read(ref _warnings);

    /// <summary>
    /// Adds rows read.
    /// </summary>
    /// <param name="count">The number of rows.</param>
    public void AddRowsIn(long count) => Interlocked.Add(ref _rowsIn, count);

    /// <summary>
    /// Adds rows written.
    /// </summary>
    /// <param name="count">The number of rows.</param>
    public void AddRowsOut(long count) => Interlocked.Add(ref _rowsOut, count);

    /// <summary>
    /// Adds a file read.
    /// </summary>
    public void AddFileIn() => Interlocked.Increment(ref _filesIn);

    /// <summary>
    /// Adds a file written.
    /// </summary>
    public void AddFileOut() => Interlocked.Increment(ref _filesOut);

    /// <summary>
    /// Adds a warning.
    /// </summary>
    public void AddWarning() => Interlocked.Increment(ref _warnings);
}

namespace OrchardCore.Telemetry;

/// <summary>
/// Accumulates the database operations of an operation, such as an HTTP request, across its asynchronous flow,
/// to report how many database operations it ran, and how long they took.
/// </summary>
public sealed class DatabaseTelemetryScope
{
    private static readonly AsyncLocal<DatabaseTelemetryScope> s_current = new();

    private int _operationCount;
    private long _durationTicks;

    /// <summary>
    /// Gets the scope of the current asynchronous flow, if any.
    /// </summary>
    public static DatabaseTelemetryScope Current => s_current.Value;

    /// <summary>
    /// Starts a scope for the current asynchronous flow and the flows it starts.
    /// </summary>
    public static DatabaseTelemetryScope Begin()
    {
        var scope = new DatabaseTelemetryScope();
        s_current.Value = scope;

        return scope;
    }

    /// <summary>
    /// Gets the number of database operations recorded in this scope.
    /// </summary>
    public int OperationCount => Volatile.Read(ref _operationCount);

    /// <summary>
    /// Gets the total duration of the database operations recorded in this scope.
    /// </summary>
    public TimeSpan Duration => TimeSpan.FromTicks(Interlocked.Read(ref _durationTicks));

    /// <summary>
    /// Records a database operation.
    /// </summary>
    /// <param name="duration">The duration of the operation.</param>
    public void RecordOperation(TimeSpan duration)
    {
        Interlocked.Increment(ref _operationCount);
        Interlocked.Add(ref _durationTicks, duration.Ticks);
    }
}

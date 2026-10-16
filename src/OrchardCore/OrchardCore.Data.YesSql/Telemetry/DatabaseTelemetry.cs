using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;
using OrchardCore.Telemetry;

namespace OrchardCore.Data.YesSql.Telemetry;

/// <summary>
/// Traces and measures the database commands run through YesSql, whatever the database provider.
/// </summary>
internal static partial class DatabaseTelemetry
{
    private const int MaxCachedStatements = 2000;

    // The boundaries recommended by the OpenTelemetry semantic conventions for 'db.client.operation.duration'.
    private static readonly InstrumentAdvice<double> s_durationAdvice = new()
    {
        HistogramBucketBoundaries = [0.001, 0.005, 0.01, 0.05, 0.1, 0.5, 1, 5, 10],
    };

    private static readonly ConcurrentDictionary<string, (string Operation, string Collection)> s_statements = new();

    public static Histogram<double> OperationDuration { get; } = OrchardCoreTelemetry.Meter.CreateHistogram(
        OrchardCoreTelemetryConstants.Instruments.DbClientOperationDuration,
        unit: "s",
        description: "Duration of database client operations.",
        tags: null,
        advice: s_durationAdvice);

    /// <summary>
    /// Gets whether anything listens to the database telemetry, so that connections are worth instrumenting.
    /// </summary>
    public static bool IsEnabled
    {
        get
        {
            return OrchardCoreTelemetry.ActivitySource.HasListeners() ||
                OperationDuration.Enabled ||
                DatabaseTelemetryScope.Current is not null;
        }
    }

    /// <summary>
    /// Runs a command, and records its span, its duration, and its count in the current <see cref="DatabaseTelemetryScope"/>.
    /// </summary>
    public static async Task<TResult> ExecuteAsync<TResult>(DatabaseTelemetryContext context, DbCommand command, Func<Task<TResult>> executeAsync)
    {
        var (operation, collection) = ParseStatement(command.CommandText);
        using var activity = StartActivity(context, command, operation, collection);
        var startTimestamp = Stopwatch.GetTimestamp();
        Exception exception = null;

        try
        {
            return await executeAsync();
        }
        catch (Exception ex)
        {
            exception = ex;
            OrchardCoreTelemetry.RecordException(activity, ex);

            throw;
        }
        finally
        {
            Record(context, operation, Stopwatch.GetElapsedTime(startTimestamp), exception);
        }
    }

    /// <summary>
    /// The synchronous counterpart of <see cref="ExecuteAsync{TResult}"/>.
    /// </summary>
    public static TResult Execute<TResult>(DatabaseTelemetryContext context, DbCommand command, Func<TResult> execute)
    {
        var (operation, collection) = ParseStatement(command.CommandText);
        using var activity = StartActivity(context, command, operation, collection);
        var startTimestamp = Stopwatch.GetTimestamp();
        Exception exception = null;

        try
        {
            return execute();
        }
        catch (Exception ex)
        {
            exception = ex;
            OrchardCoreTelemetry.RecordException(activity, ex);

            throw;
        }
        finally
        {
            Record(context, operation, Stopwatch.GetElapsedTime(startTimestamp), exception);
        }
    }

    /// <summary>
    /// Returns the operation, e.g. 'SELECT', and the first table of a SQL statement, either of them being
    /// <see langword="null"/> when it can't be found.
    /// </summary>
    internal static (string Operation, string Collection) ParseStatement(string commandText)
    {
        if (string.IsNullOrWhiteSpace(commandText))
        {
            return default;
        }

        if (s_statements.TryGetValue(commandText, out var statement))
        {
            return statement;
        }

        var operation = OperationRegex().Match(commandText);
        var collection = CollectionRegex().Match(commandText);

        statement = (
            operation.Success ? operation.Groups[1].Value.ToUpperInvariant() : null,
            collection.Success ? collection.Groups[1].Value : null);

        // The commands of YesSql are parameterized, so they repeat: a bounded cache avoids parsing them again.
        if (s_statements.Count < MaxCachedStatements)
        {
            s_statements.TryAdd(commandText, statement);
        }

        return statement;
    }

    private static Activity StartActivity(DatabaseTelemetryContext context, DbCommand command, string operation, string collection)
    {
        if (!OrchardCoreTelemetry.ActivitySource.HasListeners())
        {
            return null;
        }

        // The span name follows the OpenTelemetry semantic conventions: "{operation} {table}".
        var name = (operation, collection) switch
        {
            (not null, not null) => $"{operation} {collection}",
            (not null, null) => operation,
            _ => context.SystemName,
        };

        var activity = OrchardCoreTelemetry.ActivitySource.StartActivity(name, ActivityKind.Client);
        if (activity is not { IsAllDataRequested: true })
        {
            return activity;
        }

        activity.SetTag(OrchardCoreTelemetryConstants.Attributes.DbSystemName, context.SystemName);
        activity.SetTag(OrchardCoreTelemetryConstants.Attributes.TenantName, context.TenantName);
        activity.SetTag(OrchardCoreTelemetryConstants.Attributes.DbOperationName, operation);
        activity.SetTag(OrchardCoreTelemetryConstants.Attributes.DbCollectionName, collection);

        var database = command.Connection?.Database;
        if (!string.IsNullOrEmpty(database))
        {
            activity.SetTag(OrchardCoreTelemetryConstants.Attributes.DbNamespace, database);
        }

        if (context.IncludeQueryText)
        {
            activity.SetTag(OrchardCoreTelemetryConstants.Attributes.DbQueryText, command.CommandText);
        }

        return activity;
    }

    private static void Record(DatabaseTelemetryContext context, string operation, TimeSpan elapsed, Exception exception)
    {
        DatabaseTelemetryScope.Current?.RecordOperation(elapsed);

        if (!OperationDuration.Enabled)
        {
            return;
        }

        // No table nor SQL text: they would multiply the number of metric series.
        var tags = new TagList
        {
            { OrchardCoreTelemetryConstants.Attributes.DbSystemName, context.SystemName },
            { OrchardCoreTelemetryConstants.Attributes.TenantName, context.TenantName },
        };

        if (operation is not null)
        {
            tags.Add(OrchardCoreTelemetryConstants.Attributes.DbOperationName, operation);
        }

        if (exception is not null)
        {
            tags.Add(OrchardCoreTelemetryConstants.Attributes.ErrorType, exception.GetType().FullName);
        }

        OperationDuration.Record(elapsed.TotalSeconds, tags);
    }

    // The first keyword of the statement, after any leading comment.
    [GeneratedRegex(@"^\s*(?:(?:--[^\n]*\n|/\*.*?\*/)\s*)*([A-Za-z]+)", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex OperationRegex();

    // The first table named after FROM, INTO, UPDATE, TABLE or INDEX ... ON, without its schema nor its quotes or brackets.
    [GeneratedRegex(@"\b(?:FROM|INTO|UPDATE|TABLE|INDEX\s+\S+\s+ON)\s+(?:IF\s+(?:NOT\s+)?EXISTS\s+)?(?:[\[""`]?[\w$]+[\]""`]?\.)*[\[""`]?([\w$]+)[\]""`]?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CollectionRegex();
}

/// <summary>
/// What the database telemetry of a tenant needs to know about its connections.
/// </summary>
internal sealed record DatabaseTelemetryContext(string TenantName, string SystemName, bool IncludeQueryText);

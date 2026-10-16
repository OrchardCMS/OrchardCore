using System.Diagnostics.Metrics;

namespace OrchardCore.Telemetry;

/// <summary>
/// The metrics of the database operations run by each HTTP request.
/// </summary>
internal static class RequestDatabaseMetrics
{
    public static Histogram<int> Operations { get; } = OrchardCoreTelemetry.Meter.CreateHistogram(
        OrchardCoreTelemetryConstants.Instruments.RequestDbOperations,
        unit: "{operation}",
        description: "Number of database operations run by an HTTP request.",
        tags: null,
        advice: new InstrumentAdvice<int> { HistogramBucketBoundaries = [0, 1, 2, 5, 10, 20, 50, 100, 200, 500] });

    public static Histogram<double> Duration { get; } = OrchardCoreTelemetry.Meter.CreateHistogram(
        OrchardCoreTelemetryConstants.Instruments.RequestDbDuration,
        unit: "s",
        description: "Total duration of the database operations run by an HTTP request.",
        tags: null,
        advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = [0.001, 0.005, 0.01, 0.05, 0.1, 0.5, 1, 5, 10] });
}

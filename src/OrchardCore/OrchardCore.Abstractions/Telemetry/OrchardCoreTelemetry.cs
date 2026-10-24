using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace OrchardCore.Telemetry;

/// <summary>
/// The <see cref="System.Diagnostics.ActivitySource"/> and <see cref="System.Diagnostics.Metrics.Meter"/> used by
/// Orchard Core to emit traces and metrics. They can be collected by OpenTelemetry, 'dotnet-counters', 'dotnet-trace'
/// or any other listener, and modules can use them to emit their own telemetry.
/// </summary>
public static class OrchardCoreTelemetry
{
    private static readonly string s_version = typeof(OrchardCoreTelemetry).Assembly.GetName().Version?.ToString();

    // Tenant builds and background tasks may take from milliseconds to minutes.
    private static readonly InstrumentAdvice<double> s_durationAdvice = new()
    {
        HistogramBucketBoundaries = [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10, 30, 60, 120, 300],
    };

    /// <summary>
    /// Gets the <see cref="System.Diagnostics.ActivitySource"/> of the traces emitted by Orchard Core.
    /// </summary>
    public static ActivitySource ActivitySource { get; } = new(OrchardCoreTelemetryConstants.ActivitySourceName, s_version);

    /// <summary>
    /// Gets the <see cref="System.Diagnostics.Metrics.Meter"/> of the metrics emitted by Orchard Core.
    /// </summary>
    public static Meter Meter { get; } = new(OrchardCoreTelemetryConstants.MeterName, s_version);

    internal static Histogram<double> TenantContainerDuration { get; } = Meter.CreateHistogram(
        OrchardCoreTelemetryConstants.Instruments.TenantContainerDuration,
        unit: "s",
        description: "Duration of building the service container of a tenant.",
        tags: null,
        advice: s_durationAdvice);

    internal static Histogram<double> TenantPipelineDuration { get; } = Meter.CreateHistogram(
        OrchardCoreTelemetryConstants.Instruments.TenantPipelineDuration,
        unit: "s",
        description: "Duration of building the request pipeline of a tenant.",
        tags: null,
        advice: s_durationAdvice);

    internal static Histogram<double> TenantActivationDuration { get; } = Meter.CreateHistogram(
        OrchardCoreTelemetryConstants.Instruments.TenantActivationDuration,
        unit: "s",
        description: "Duration of activating a tenant.",
        tags: null,
        advice: s_durationAdvice);

    internal static Counter<long> TenantReleases { get; } = Meter.CreateCounter<long>(
        OrchardCoreTelemetryConstants.Instruments.TenantReleases,
        unit: "{release}",
        description: "Number of times a tenant was released, so that it gets rebuilt on its next use.");

    internal static Histogram<double> BackgroundTaskDuration { get; } = Meter.CreateHistogram(
        OrchardCoreTelemetryConstants.Instruments.BackgroundTaskDuration,
        unit: "s",
        description: "Duration of running a background task.",
        tags: null,
        advice: s_durationAdvice);

    /// <summary>
    /// Marks an <see cref="Activity"/> as failed by the provided <see cref="Exception"/>.
    /// </summary>
    /// <param name="activity">The <see cref="Activity"/> to mark as failed, can be <see langword="null"/> when nothing listens.</param>
    /// <param name="exception">The <see cref="Exception"/> that ended the operation.</param>
    public static void RecordException(Activity activity, Exception exception)
    {
        if (activity is null)
        {
            return;
        }

        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.SetTag(OrchardCoreTelemetryConstants.Attributes.ErrorType, exception.GetType().FullName);
        activity.AddException(exception);
    }

    /// <summary>
    /// Starts an <see cref="Activity"/> related to a tenant, if anything listens.
    /// </summary>
    internal static Activity StartTenantActivity(string name, string tenantName, ActivityKind kind = ActivityKind.Internal)
    {
        var activity = ActivitySource.StartActivity(name, kind);
        activity?.SetTag(OrchardCoreTelemetryConstants.Attributes.TenantName, tenantName);

        return activity;
    }

    /// <summary>
    /// Records the time elapsed since <paramref name="startTimestamp"/> on a duration histogram, if anything listens.
    /// </summary>
    internal static void RecordDuration(
        Histogram<double> histogram,
        long startTimestamp,
        string tenantName,
        Exception exception = null,
        KeyValuePair<string, object> extraTag = default)
    {
        if (!histogram.Enabled)
        {
            return;
        }

        var tags = new TagList
        {
            { OrchardCoreTelemetryConstants.Attributes.TenantName, tenantName },
        };

        if (extraTag.Key is not null)
        {
            tags.Add(extraTag);
        }

        if (exception is not null)
        {
            tags.Add(OrchardCoreTelemetryConstants.Attributes.ErrorType, exception.GetType().FullName);
        }

        histogram.Record(Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds, tags);
    }
}

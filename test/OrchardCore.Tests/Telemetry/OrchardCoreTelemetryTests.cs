using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OrchardCore.Telemetry;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Telemetry;

public class OrchardCoreTelemetryTests
{
    [Fact]
    public async Task InitializeTenant_WithListeners_EmitsTenantSpansAndMetrics()
    {
        var activities = new ConcurrentQueue<Activity>();
        using var activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == OrchardCoreTelemetryConstants.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activities.Enqueue,
        };
        ActivitySource.AddActivityListener(activityListener);

        var measurements = new ConcurrentQueue<(string Instrument, string TenantName)>();
        using var meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == OrchardCoreTelemetryConstants.MeterName)
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            },
        };
        meterListener.SetMeasurementEventCallback<double>((instrument, _, tags, _) =>
        {
            foreach (var tag in tags)
            {
                if (tag.Key == OrchardCoreTelemetryConstants.Attributes.TenantName)
                {
                    measurements.Enqueue((instrument.Name, tag.Value as string));
                }
            }
        });
        meterListener.Start();

        using var context = new SiteContext();
        await context.InitializeAsync();

        // Other tests may create tenants concurrently, only consider the telemetry of this tenant.
        var tenantActivities = activities
            .Where(activity => activity.GetTagItem(OrchardCoreTelemetryConstants.Attributes.TenantName) as string == context.TenantName)
            .ToArray();

        Assert.Contains(tenantActivities, activity => activity.OperationName == "Build tenant container");
        Assert.Contains(tenantActivities, activity => activity.OperationName == "Activate tenant");
        Assert.Contains(tenantActivities, activity => activity.OperationName == "Execute recipe Blog");
        Assert.Contains(tenantActivities, activity =>
            activity.OperationName == "Execute recipe step feature" &&
            activity.GetTagItem(OrchardCoreTelemetryConstants.Attributes.RecipeStepName) as string == "feature");
        // Database commands may fail on purpose, e.g. a probe caught by YesSql: only the internal operations must succeed.
        Assert.All(
            tenantActivities.Where(activity => activity.Kind == ActivityKind.Internal),
            activity => Assert.NotEqual(ActivityStatusCode.Error, activity.Status));

        Assert.Contains((OrchardCoreTelemetryConstants.Instruments.TenantContainerDuration, context.TenantName), measurements);
        Assert.Contains((OrchardCoreTelemetryConstants.Instruments.TenantActivationDuration, context.TenantName), measurements);
    }

    [Fact]
    public async Task TenantActivityProcessor_InTenantScope_TagsSpanWithTenantName()
    {
        using var source = new ActivitySource(nameof(TenantActivityProcessor_InTenantScope_TagsSpanWithTenantName));
        using var listener = new ActivityListener
        {
            ShouldListenTo = activitySource => activitySource == source,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
        };
        ActivitySource.AddActivityListener(listener);

        var processor = new TenantActivityProcessor();

        using var context = new SiteContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(scope =>
        {
            using var activity = source.StartActivity("In tenant scope");
            processor.OnStart(activity);

            Assert.Equal(context.TenantName, activity.GetTagItem(OrchardCoreTelemetryConstants.Attributes.TenantName));

            return Task.CompletedTask;
        });

        using var hostActivity = source.StartActivity("Out of tenant scope");
        processor.OnStart(hostActivity);

        Assert.Null(hostActivity.GetTagItem(OrchardCoreTelemetryConstants.Attributes.TenantName));
    }

    [Fact]
    public async Task TenantLogRecordProcessor_InTenantScope_AddsTenantNameBeforeLaterProcessors()
    {
        var records = new ConcurrentQueue<(string Message, string TenantName)>();
        using var loggerFactory = LoggerFactory.Create(logging => logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.AddProcessor(new TenantLogRecordProcessor());
            options.AddProcessor(new CapturingLogRecordProcessor(records));
        }));
        var logger = loggerFactory.CreateLogger<OrchardCoreTelemetryTests>();

        using var context = new SiteContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(scope =>
        {
            logger.LogWarning("In tenant scope {Value}", 1);

            return Task.CompletedTask;
        });

        logger.LogWarning("Out of tenant scope");

        Assert.Contains(("In tenant scope 1", context.TenantName), records);
        Assert.Contains(("Out of tenant scope", null), records);
    }

    private sealed class CapturingLogRecordProcessor : BaseProcessor<LogRecord>
    {
        private readonly ConcurrentQueue<(string Message, string TenantName)> _records;

        public CapturingLogRecordProcessor(ConcurrentQueue<(string Message, string TenantName)> records)
        {
            _records = records;
        }

        public override void OnEnd(LogRecord data)
        {
            var tenantName = data.Attributes?
                .FirstOrDefault(attribute => attribute.Key == OrchardCoreTelemetryConstants.Attributes.TenantName)
                .Value as string;

            _records.Enqueue((data.FormattedMessage, tenantName));
        }
    }
}

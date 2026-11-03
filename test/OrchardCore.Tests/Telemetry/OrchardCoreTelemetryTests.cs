using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
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
        Assert.All(tenantActivities, activity => Assert.NotEqual(ActivityStatusCode.Error, activity.Status));

        Assert.Contains((OrchardCoreTelemetryConstants.Instruments.TenantContainerDuration, context.TenantName), measurements);
        Assert.Contains((OrchardCoreTelemetryConstants.Instruments.TenantActivationDuration, context.TenantName), measurements);
    }

    [Fact]
    public async Task MeasureTenantOperationAsync_OperationThrows_RecordsTheFailure()
    {
        var tenantName = Guid.NewGuid().ToString("n");
        var activities = new ConcurrentQueue<Activity>();
        using var activityListener = CreateActivityListener(activities);

        var errorTypes = new ConcurrentQueue<string>();
        using var meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument == OrchardCoreTelemetry.TenantActivationDuration)
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            },
        };
        meterListener.SetMeasurementEventCallback<double>((_, _, tags, _) =>
        {
            if (GetTag(tags, OrchardCoreTelemetryConstants.Attributes.TenantName) == tenantName)
            {
                errorTypes.Enqueue(GetTag(tags, OrchardCoreTelemetryConstants.Attributes.ErrorType));
            }
        });
        meterListener.Start();

        await Assert.ThrowsAsync<InvalidOperationException>(() => OrchardCoreTelemetry.MeasureTenantOperationAsync(
            "Failing operation",
            OrchardCoreTelemetry.TenantActivationDuration,
            tenantName,
            () => throw new InvalidOperationException("Failed")));

        var activity = Assert.Single(activities, activity => activity.GetTagItem(OrchardCoreTelemetryConstants.Attributes.TenantName) as string == tenantName);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("Failed", activity.StatusDescription);
        Assert.Equal(typeof(InvalidOperationException).FullName, activity.GetTagItem(OrchardCoreTelemetryConstants.Attributes.ErrorType));
        Assert.Contains(activity.Events, activityEvent => activityEvent.Name == "exception");
        Assert.Equal(typeof(InvalidOperationException).FullName, Assert.Single(errorTypes));
    }

    [Fact]
    public async Task ExecuteRecipe_StepWithErrors_RecordsTheFailure()
    {
        var activities = new ConcurrentQueue<Activity>();
        using var activityListener = CreateActivityListener(activities);

        using var context = new BlogContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var recipeDescriptor = new RecipeDescriptor
            {
                Name = "Failing",
                RecipeFileInfo = new EmbeddedFileProvider(GetType().Assembly).GetFileInfo("Recipes.RecipeFiles.recipe6.json"),
            };

            await Assert.ThrowsAsync<RecipeExecutionException>(() => scope.ServiceProvider
                .GetRequiredService<IRecipeExecutor>()
                .ExecuteAsync(Guid.NewGuid().ToString("n"), recipeDescriptor, new Dictionary<string, object>(), CancellationToken.None));
        });

        var tenantActivities = activities
            .Where(activity =>
                activity.GetTagItem(OrchardCoreTelemetryConstants.Attributes.TenantName) as string == context.TenantName &&
                activity.GetTagItem(OrchardCoreTelemetryConstants.Attributes.RecipeName) as string == "Failing")
            .ToArray();

        var step = Assert.Single(tenantActivities, activity => activity.OperationName == "Execute recipe step ContentDefinition");
        Assert.Equal(ActivityStatusCode.Error, step.Status);
        Assert.Contains("The part name cannot be null or empty.", step.StatusDescription);
        Assert.Equal(typeof(RecipeExecutionException).FullName, step.GetTagItem(OrchardCoreTelemetryConstants.Attributes.ErrorType));

        var recipe = Assert.Single(tenantActivities, activity => activity.OperationName == "Execute recipe Failing");
        Assert.Equal(ActivityStatusCode.Error, recipe.Status);
        Assert.Equal(typeof(RecipeExecutionException).FullName, recipe.GetTagItem(OrchardCoreTelemetryConstants.Attributes.ErrorType));
        Assert.Contains(recipe.Events, activityEvent => activityEvent.Name == "exception");
    }

    [Fact]
    public async Task TenantReleases_ReleaseReloadAndRemove_CountsEachReason()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        var reasons = new ConcurrentQueue<string>();
        using var meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument == OrchardCoreTelemetry.TenantReleases)
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            },
        };
        meterListener.SetMeasurementEventCallback<long>((_, _, tags, _) =>
        {
            if (GetTag(tags, OrchardCoreTelemetryConstants.Attributes.TenantName) == context.TenantName)
            {
                reasons.Enqueue(GetTag(tags, OrchardCoreTelemetryConstants.Attributes.TenantReleaseReason));
            }
        });
        meterListener.Start();

        var shellHost = SiteContext.ShellHost;

        await shellHost.GetOrCreateShellContextAsync(GetSettings());
        await shellHost.ReleaseShellContextAsync(GetSettings(), eventSource: false);

        // Nothing is built after a release, so there is nothing else to count.
        await shellHost.ReleaseShellContextAsync(GetSettings(), eventSource: false);

        await shellHost.GetOrCreateShellContextAsync(GetSettings());
        await shellHost.ReloadShellContextAsync(GetSettings(), eventSource: false);

        // Only a disabled tenant can be removed, which reloads it.
        await shellHost.GetOrCreateShellContextAsync(GetSettings());
        await shellHost.UpdateShellSettingsAsync(new ShellSettings(GetSettings()).AsDisabled());

        await shellHost.GetOrCreateShellContextAsync(GetSettings());
        await shellHost.RemoveShellSettingsAsync(GetSettings());

        Assert.Equal(["release", "reload", "reload", "remove"], reasons);

        ShellSettings GetSettings()
        {
            Assert.True(shellHost.TryGetSettings(context.TenantName, out var settings));

            return settings;
        }
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

    // Only considers the activities of the 'OrchardCore' source, which concurrent tests also use.
    private static ActivityListener CreateActivityListener(ConcurrentQueue<Activity> activities)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == OrchardCoreTelemetryConstants.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activities.Enqueue,
        };
        ActivitySource.AddActivityListener(listener);

        return listener;
    }

    private static string GetTag(ReadOnlySpan<KeyValuePair<string, object>> tags, string name)
    {
        foreach (var tag in tags)
        {
            if (tag.Key == name)
            {
                return tag.Value as string;
            }
        }

        return null;
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

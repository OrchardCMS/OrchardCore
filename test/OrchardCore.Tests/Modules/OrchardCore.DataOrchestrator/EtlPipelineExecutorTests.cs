using System.Text.Json.Nodes;
using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Models;
using OrchardCore.DataOrchestrator.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.DataOrchestrator;

public class EtlPipelineExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_RunsSourceTransformLoad_AndReportsCounts()
    {
        var source = new JsonSource
        {
            Data = "[{\"name\":\"a\",\"active\":\"true\"},{\"name\":\"b\",\"active\":\"false\"},{\"name\":\"c\",\"active\":\"true\"}]",
        };

        var filter = new FilterTransform
        {
            Field = "active",
            Operator = "equals",
            Value = "true",
        };

        var load = new CaptureLoad();

        var log = await ExecuteAsync(source, filter, load);

        Assert.Equal("Success", log.Status);
        Assert.Equal(0, log.ErrorCount);
        Assert.Equal(2, load.Captured.Count);
        Assert.Equal(2, log.RecordsLoaded);
        Assert.Equal(2, log.RecordsProcessed);
        Assert.Contains(load.Captured, r => r["name"].GetValue<string>() == "a");
        Assert.Contains(load.Captured, r => r["name"].GetValue<string>() == "c");
        Assert.DoesNotContain(load.Captured, r => r["name"].GetValue<string>() == "b");
    }

    [Fact]
    public async Task ExecuteAsync_WithNoStartActivity_Fails()
    {
        var executor = new EtlPipelineExecutor(
            new FakeActivityLibrary([]),
            new ServiceCollection().BuildServiceProvider(),
            NullLogger<EtlPipelineExecutor>.Instance);

        var pipeline = new EtlPipelineDefinition
        {
            PipelineId = "empty",
            Name = "Empty",
        };

        var log = await executor.ExecuteAsync(pipeline, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Failed", log.Status);
        Assert.Contains(log.Errors, e => e.Contains("start", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoadFails_MarksRunFailed()
    {
        var source = new JsonSource
        {
            Data = "[{\"name\":\"a\"}]",
        };

        var load = new FailingLoad();

        var log = await ExecuteAsync(source, null, load);

        Assert.Equal("Failed", log.Status);
        Assert.True(log.ErrorCount > 0);
    }

    [Fact]
    public async Task ExecuteAsync_WithFanOut_PreservesBranchDataStreams()
    {
        var source = new JsonSource
        {
            Data = "[{\"name\":\"a\",\"active\":\"true\"},{\"name\":\"b\",\"active\":\"false\"}]",
        };

        var activeFilter = new FilterTransform
        {
            Field = "active",
            Operator = "equals",
            Value = "true",
        };

        var inactiveFilter = new FilterTransform
        {
            Field = "active",
            Operator = "equals",
            Value = "false",
        };

        var activeLoad = new CaptureLoad("ActiveLoad");
        var inactiveLoad = new CaptureLoad("InactiveLoad");

        var sourceRecord = CreateRecord(source, isStart: true);
        var activeFilterRecord = CreateRecord(activeFilter);
        var inactiveFilterRecord = CreateRecord(inactiveFilter);
        var activeLoadRecord = CreateRecord(activeLoad);
        var inactiveLoadRecord = CreateRecord(inactiveLoad);

        var pipeline = new EtlPipelineDefinition
        {
            PipelineId = "p1",
            Name = "Fan-out pipeline",
            Activities =
            [
                sourceRecord,
                activeFilterRecord,
                inactiveFilterRecord,
                activeLoadRecord,
                inactiveLoadRecord,
            ],
            Transitions =
            [
                Transition(sourceRecord, activeFilterRecord),
                Transition(activeFilterRecord, activeLoadRecord),
                Transition(sourceRecord, inactiveFilterRecord),
                Transition(inactiveFilterRecord, inactiveLoadRecord),
            ],
        };

        var executor = new EtlPipelineExecutor(
            new FakeActivityLibrary([source, activeFilter, activeLoad, inactiveLoad]),
            new ServiceCollection().BuildServiceProvider(),
            NullLogger<EtlPipelineExecutor>.Instance);

        var log = await executor.ExecuteAsync(pipeline, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Success", log.Status);
        Assert.Single(activeLoad.Captured);
        Assert.Single(inactiveLoad.Captured);
        Assert.Equal("a", activeLoad.Captured[0]["name"].GetValue<string>());
        Assert.Equal("b", inactiveLoad.Captured[0]["name"].GetValue<string>());
        Assert.Equal(2, log.RecordsProcessed);
        Assert.Equal(2, log.RecordsLoaded);
    }

    [Fact]
    public async Task ExecuteAsync_WithCycle_FailsWithoutRepeatingActivitiesIndefinitely()
    {
        var first = new CountingActivity("First");
        var second = new CountingActivity("Second");
        var firstRecord = CreateRecord(first, isStart: true);
        var secondRecord = CreateRecord(second);

        var pipeline = new EtlPipelineDefinition
        {
            PipelineId = "cycle",
            Name = "Cycle",
            Activities = [firstRecord, secondRecord],
            Transitions =
            [
                Transition(firstRecord, secondRecord),
                Transition(secondRecord, firstRecord),
            ],
        };

        var executor = new EtlPipelineExecutor(
            new FakeActivityLibrary([first, second]),
            new ServiceCollection().BuildServiceProvider(),
            NullLogger<EtlPipelineExecutor>.Instance);

        var log = await executor.ExecuteAsync(pipeline, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Failed", log.Status);
        Assert.Equal(1, log.ErrorCount);
        Assert.Contains(log.Errors, error => error.Contains("Cycle", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(1, first.ExecuteCount);
        Assert.Equal(1, second.ExecuteCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenActivityConfigurationIsInvalid_FailsWithoutConsumingLoad()
    {
        var source = new JsonSource
        {
            Data = "[{\"date\":\"2026-06-29T12:00:00Z\"}]",
        };

        var format = new FormatValueTransform
        {
            Field = "date",
            FormatType = FormatValueTransform.ConvertUtcToTimeZoneFormat,
            TimeZoneId = "Invalid/TimeZone",
        };

        var load = new CaptureLoad();

        var log = await ExecuteAsync(source, format, load);

        Assert.Equal("Failed", log.Status);
        Assert.Equal(1, log.ErrorCount);
        Assert.Contains(log.Errors, error => error.Contains("time zone", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(load.Captured);
    }

    private static async Task<EtlExecutionLog> ExecuteAsync(IEtlActivity source, IEtlActivity transform, IEtlActivity load)
    {
        var activities = new List<IEtlActivity> { source, load };
        var records = new List<EtlActivityRecord>();

        var sourceRecord = CreateRecord(source, isStart: true);
        records.Add(sourceRecord);

        EtlActivityRecord transformRecord = null;

        if (transform != null)
        {
            activities.Add(transform);
            transformRecord = CreateRecord(transform);
            records.Add(transformRecord);
        }

        var loadRecord = CreateRecord(load);
        records.Add(loadRecord);

        var transitions = new List<EtlTransition>();

        if (transformRecord != null)
        {
            transitions.Add(Transition(sourceRecord, transformRecord));
            transitions.Add(Transition(transformRecord, loadRecord));
        }
        else
        {
            transitions.Add(Transition(sourceRecord, loadRecord));
        }

        var pipeline = new EtlPipelineDefinition
        {
            PipelineId = "p1",
            Name = "Test pipeline",
            Activities = records,
            Transitions = transitions,
        };

        var executor = new EtlPipelineExecutor(
            new FakeActivityLibrary(activities),
            new ServiceCollection().BuildServiceProvider(),
            NullLogger<EtlPipelineExecutor>.Instance);

        return await executor.ExecuteAsync(pipeline);
    }

    private static EtlActivityRecord CreateRecord(IEtlActivity activity, bool isStart = false, string name = null)
    {
        return new EtlActivityRecord
        {
            ActivityId = Guid.NewGuid().ToString("n"),
            Name = name ?? activity.Name,
            Properties = activity.Properties,
            IsStart = isStart,
        };
    }

    private static EtlTransition Transition(EtlActivityRecord source, EtlActivityRecord destination)
    {
        return new EtlTransition
        {
            SourceActivityId = source.ActivityId,
            SourceOutcomeName = "Done",
            DestinationActivityId = destination.ActivityId,
        };
    }

    private sealed class FakeActivityLibrary : IEtlActivityLibrary
    {
        private readonly Dictionary<string, IEtlActivity> _activities;

        public FakeActivityLibrary(IEnumerable<IEtlActivity> activities)
        {
            _activities = activities.ToDictionary(x => x.Name);
        }

        public IEnumerable<IEtlActivity> ListActivities()
        {
            return _activities.Values;
        }

        public IEnumerable<string> ListCategories()
        {
            return _activities.Values.Select(x => x.Category).Distinct();
        }

        public IEtlActivity GetActivityByName(string name)
        {
            return _activities.GetValueOrDefault(name);
        }

        public IEtlActivity InstantiateActivity(string name)
        {
            return _activities.GetValueOrDefault(name);
        }
    }

    private sealed class CountingActivity : EtlActivity
    {
        public CountingActivity(string name)
        {
            Name = name;
        }

        public override string Name { get; }

        public override string DisplayText => Name;

        public override string Category => "Tests";

        public int ExecuteCount { get; private set; }

        public override IEnumerable<EtlOutcome> GetPossibleOutcomes()
        {
            return [new EtlOutcome("Done")];
        }

        public override Task<EtlActivityResult> ExecuteAsync(EtlExecutionContext context)
        {
            ExecuteCount++;

            return Task.FromResult(EtlActivityResult.Success("Done"));
        }
    }

    private sealed class CaptureLoad : EtlLoadActivity
    {
        public CaptureLoad(string name = nameof(CaptureLoad))
        {
            Name = name;
        }

        public List<JsonObject> Captured { get; } = [];

        public override string Name { get; }

        public override string DisplayText => "Capture";

        public override IEnumerable<EtlOutcome> GetPossibleOutcomes()
        {
            return [new EtlOutcome("Done")];
        }

        public override async Task<EtlActivityResult> ExecuteAsync(EtlExecutionContext context)
        {
            if (context.DataStream == null)
            {
                return EtlActivityResult.Failure("No data stream available.");
            }

            var count = 0;

            await foreach (var record in context.DataStream.WithCancellation(context.CancellationToken))
            {
                Captured.Add(record);
                count++;
            }

            context.IncrementRecordsProcessed(count);
            context.IncrementRecordsLoaded(count);

            return EtlActivityResult.Success("Done");
        }
    }

    private sealed class FailingLoad : EtlLoadActivity
    {
        public override string Name => nameof(FailingLoad);

        public override string DisplayText => "Failing";

        public override IEnumerable<EtlOutcome> GetPossibleOutcomes()
        {
            return [new EtlOutcome("Done"), new EtlOutcome("Failed")];
        }

        public override Task<EtlActivityResult> ExecuteAsync(EtlExecutionContext context)
        {
            return Task.FromResult(EtlActivityResult.Failure("Intentional failure."));
        }
    }
}

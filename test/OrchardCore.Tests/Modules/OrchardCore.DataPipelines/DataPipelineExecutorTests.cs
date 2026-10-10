using OrchardCore.DataPipelines;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.Tests.Modules.OrchardCore.DataSources;
using static OrchardCore.Tests.Modules.OrchardCore.DataPipelines.TestSteps;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class DataPipelineExecutorTests : IDisposable
{
    private readonly TestCollector _collector = new();
    private readonly string _temp = Path.Combine(Path.GetTempPath(), "oc-pipelines-tests", Guid.NewGuid().ToString("n"));

    public void Dispose()
    {
        if (Directory.Exists(_temp))
        {
            Directory.Delete(_temp, recursive: true);
        }
    }

    [Fact]
    public async Task AnalyzeAsync_UnknownStepType_ReportsError()
    {
        // Arrange
        var definition = Define([Step("a", "Missing")], []);

        // Act
        var analysis = await CreateAnalyzer().AnalyzeAsync(definition, null, null, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(analysis.HasErrors);
        Assert.Contains(analysis.Issues, issue => issue.StepId == "a" && issue.Severity == DataPipelineIssueSeverity.Error);
    }

    [Fact]
    public async Task AnalyzeAsync_Cycle_ReportsError()
    {
        // Arrange
        var definition = Define(
            [Step("s", nameof(NumbersSource), 1), Step("u", nameof(UnionTransform)), Step("d", nameof(DoubleTransform))],
            [Connect("s", "u"), Connect("u", "d"), Connect("d", "u")]);

        // Act
        var analysis = await CreateAnalyzer().AnalyzeAsync(definition, null, null, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(analysis.HasErrors);
        Assert.Contains(analysis.Issues, issue => issue.Message.Contains("loop", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task AnalyzeAsync_RequiredInputNotConnected_ReportsError()
    {
        // Arrange
        var definition = Define([Step("d", nameof(DoubleTransform))], []);

        // Act
        var analysis = await CreateAnalyzer().AnalyzeAsync(definition, null, null, TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains(analysis.Issues, issue => issue.StepId == "d" && issue.Severity == DataPipelineIssueSeverity.Error);
    }

    [Fact]
    public async Task AnalyzeAsync_RecordOutputIntoFileInput_ReportsError()
    {
        // Arrange
        var definition = Define(
            [Step("s", nameof(NumbersSource), 1), Step("f", nameof(FileCollector))],
            [Connect("s", "f")]);

        // Act
        var analysis = await CreateAnalyzer().AnalyzeAsync(definition, null, null, TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains(analysis.Issues, issue => issue.Severity == DataPipelineIssueSeverity.Error && issue.StepId == "f");
    }

    [Fact]
    public async Task AnalyzeAsync_SecondConnectionOnSingleInput_ReportsError()
    {
        // Arrange
        var definition = Define(
            [Step("s1", nameof(NumbersSource), 1), Step("s2", nameof(NumbersSource), 1), Step("d", nameof(DoubleTransform)), Step("c", nameof(CollectDestination))],
            [Connect("s1", "d"), Connect("s2", "d"), Connect("d", "c")]);

        // Act
        var analysis = await CreateAnalyzer().AnalyzeAsync(definition, null, null, TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains(analysis.Issues, issue => issue.Severity == DataPipelineIssueSeverity.Error && issue.StepId == "d");
    }

    [Fact]
    public async Task AnalyzeAsync_ValidPipeline_ResolvesFieldsThroughSteps()
    {
        // Arrange
        var definition = Define(
            [Step("s", nameof(NumbersSource), 1), Step("d", nameof(DoubleTransform)), Step("c", nameof(CollectDestination))],
            [Connect("s", "d"), Connect("d", "c")]);

        // Act
        var analysis = await CreateAnalyzer().AnalyzeAsync(definition, null, null, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(analysis.HasErrors);
        Assert.Equal(["s", "d", "c"], analysis.Order);
        Assert.Equal(["Id", "Double"], analysis.GetStep("d").Outputs[DataPipelinePort.Output].Select(field => field.Name));
        Assert.Equal(["Id", "Double"], analysis.GetStep("c").GetInputFields(DataPipelinePort.Input).Select(field => field.Name));
    }

    [Fact]
    public async Task AnalyzeAsync_UnusedOutput_ReportsWarning()
    {
        // Arrange
        var definition = Define([Step("s", nameof(NumbersSource), 1)], []);

        // Act
        var analysis = await CreateAnalyzer().AnalyzeAsync(definition, null, null, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(analysis.HasErrors);
        Assert.Contains(analysis.Issues, issue => issue.StepId == "s" && issue.Severity == DataPipelineIssueSeverity.Warning);
    }

    [Fact]
    public async Task ExecuteAsync_LinearPipeline_DeliversEveryRowAndCountsThem()
    {
        // Arrange
        var definition = Define(
            [Step("s", nameof(NumbersSource), 10_000), Step("d", nameof(DoubleTransform)), Step("c", nameof(CollectDestination))],
            [Connect("s", "d"), Connect("d", "c")]);

        // Act
        var result = await CreateExecutor().ExecuteAsync(definition, CreateRun(TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal(DataPipelineRunStatus.Succeeded, result.Status);
        var rows = _collector.Rows("c");
        Assert.Equal(10_000, rows.Count);
        Assert.Equal(20_000L, rows.Max(row => (long)row[1]));
        Assert.Equal(10_000, result.Steps["s"].Metrics.RowsOut);
        Assert.Equal(10_000, result.Steps["d"].Metrics.RowsIn);
        Assert.Equal(10_000, result.Steps["c"].Metrics.RowsIn);
        Assert.All(result.Steps.Values, step => Assert.Equal(DataPipelineStepStatus.Succeeded, step.Status));
        Assert.Contains(result.Deliveries, delivery => delivery.StepId == "c");
    }

    [Fact]
    public async Task ExecuteAsync_StepScope_RunsEachStepWithItsOwnServices()
    {
        // Arrange
        var definition = Define(
            [Step("s", nameof(NumbersSource), 10), Step("d", nameof(DoubleTransform)), Step("c", nameof(CollectDestination))],
            [Connect("s", "d"), Connect("d", "c")]);
        var scoped = new ConcurrentBag<string>();
        var run = CreateRun(TestContext.Current.CancellationToken);
        run.StepScope = async (step, work) =>
        {
            scoped.Add(step.StepId);
            await using var services = new ServiceCollection().BuildServiceProvider();
            await work(services);
        };

        // Act
        var result = await CreateExecutor().ExecuteAsync(definition, run);

        // Assert
        Assert.Equal(DataPipelineRunStatus.Succeeded, result.Status);
        Assert.Equal(["c", "d", "s"], scoped.Order(StringComparer.Ordinal));
        Assert.Equal(10, _collector.Rows("c").Count);
    }

    [Fact]
    public async Task ExecuteAsync_FanOut_ReadsSourceOnceAndFeedsEveryBranch()
    {
        // Arrange
        var steps = TestSteps.All(_collector);
        var source = steps.OfType<NumbersSource>().Single();
        var definition = Define(
            [Step("s", nameof(NumbersSource), 1_000), Step("fast", nameof(CollectDestination)), Step("slow", nameof(CollectDestination), 1)],
            [Connect("s", "fast"), Connect("s", "slow")]);

        // Act
        var result = await CreateExecutor(steps).ExecuteAsync(definition, CreateRun(TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal(DataPipelineRunStatus.Succeeded, result.Status);
        Assert.Equal(1, source.Executions);
        Assert.Equal(1_000, _collector.Rows("fast").Count);
        Assert.Equal(1_000, _collector.Rows("slow").Count);
    }

    [Fact]
    public async Task ExecuteAsync_DiamondIntoMultiInputStep_CompletesWithoutDeadlock()
    {
        // Arrange
        var definition = Define(
            [
                Step("s", nameof(NumbersSource), 5_000),
                Step("left", nameof(DoubleTransform)),
                Step("right", nameof(DoubleTransform)),
                Step("u", nameof(UnionTransform)),
                Step("c", nameof(CollectDestination)),
            ],
            [Connect("s", "left"), Connect("s", "right"), Connect("left", "u"), Connect("right", "u"), Connect("u", "c")]);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var result = await CreateExecutor().ExecuteAsync(definition, CreateRun(timeout.Token));

        // Assert
        Assert.Equal(DataPipelineRunStatus.Succeeded, result.Status);
        Assert.Equal(10_000, _collector.Rows("c").Count);
    }

    [Fact]
    public async Task ExecuteAsync_StepThrows_FailsRunAndStopsOtherSteps()
    {
        // Arrange
        var definition = Define(
            [Step("s", nameof(NumbersSource), 100_000), Step("f", nameof(FailTransform), 500, "Bad row"), Step("c", nameof(CollectDestination))],
            [Connect("s", "f"), Connect("f", "c")]);

        // Act
        var result = await CreateExecutor().ExecuteAsync(definition, CreateRun(TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal(DataPipelineRunStatus.Failed, result.Status);
        Assert.Equal("f", result.FailedStepId);
        Assert.Contains("Bad row", result.Error);
        Assert.Equal(DataPipelineStepStatus.Failed, result.Steps["f"].Status);
        Assert.True(result.Steps["s"].Metrics.RowsOut < 100_000);
    }

    [Fact]
    public async Task ExecuteAsync_DownstreamStopsEarly_UpstreamStopsWithoutBlocking()
    {
        // Arrange
        var definition = Define(
            [Step("s", nameof(NumbersSource), 1_000_000), Step("l", nameof(LimitTransform), 250), Step("c", nameof(CollectDestination))],
            [Connect("s", "l"), Connect("l", "c")]);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var result = await CreateExecutor().ExecuteAsync(definition, CreateRun(timeout.Token));

        // Assert
        Assert.Equal(DataPipelineRunStatus.Succeeded, result.Status);
        Assert.Equal(250, _collector.Rows("c").Count);
        Assert.True(result.Steps["s"].Metrics.RowsOut < 1_000_000);
    }

    [Fact]
    public async Task ExecuteAsync_Cancelled_ReturnsCancelled()
    {
        // Arrange
        var definition = Define(
            [Step("s", nameof(NumbersSource), 1_000_000), Step("c", nameof(CollectDestination), 50)],
            [Connect("s", "c")]);

        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        // Act
        var result = await CreateExecutor().ExecuteAsync(definition, CreateRun(cancellation.Token));

        // Assert
        Assert.Equal(DataPipelineRunStatus.Cancelled, result.Status);
        Assert.True(_collector.Rows("c").Count < 1_000_000);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidPipeline_FailsBeforeRunningAnyStep()
    {
        // Arrange
        var steps = TestSteps.All(_collector);
        var definition = Define([Step("s", nameof(NumbersSource), 10), Step("d", nameof(DoubleTransform))], []);

        // Act
        var result = await CreateExecutor(steps).ExecuteAsync(definition, CreateRun(TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal(DataPipelineRunStatus.Failed, result.Status);
        Assert.Contains(result.Issues, issue => issue.Severity == DataPipelineIssueSeverity.Error);
        Assert.Equal(0, steps.OfType<NumbersSource>().Single().Executions);
    }

    [Fact]
    public async Task ExecuteAsync_FileSteps_PassFilesToDestinations()
    {
        // Arrange
        var definition = Define(
            [Step("s", nameof(NumbersSource), 3), Step("m", nameof(FileMaker)), Step("f", nameof(FileCollector))],
            [Connect("s", "m"), Connect("m", "f")]);

        // Act
        var result = await CreateExecutor().ExecuteAsync(definition, CreateRun(TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal(DataPipelineRunStatus.Succeeded, result.Status);
        Assert.Equal("1\n2\n3\n", _collector.Files["numbers.txt"].ReplaceLineEndings("\n"));
        Assert.Equal(1, result.Steps["m"].Metrics.FilesOut);
        Assert.Equal(1, result.Steps["f"].Metrics.FilesIn);
    }

    [Fact]
    public async Task PreviewAsync_Transform_ReturnsLimitedRowsAndSkipsDestinations()
    {
        // Arrange
        var definition = Define(
            [Step("s", nameof(NumbersSource), 10_000), Step("d", nameof(DoubleTransform)), Step("c", nameof(CollectDestination))],
            [Connect("s", "d"), Connect("d", "c")]);
        var run = CreateRun(TestContext.Current.CancellationToken);
        run.IsPreview = true;
        run.PreviewRowLimit = 25;

        // Act
        var preview = await CreateExecutor().PreviewAsync(definition, "d", run);

        // Assert
        Assert.Null(preview.Error);
        var port = Assert.Single(preview.Ports);
        Assert.Equal(DataPipelinePort.Output, port.Name);
        Assert.Equal(["Id", "Double"], port.Fields.Select(field => field.Name));
        Assert.Equal(25, port.Rows.Count);
        Assert.Equal(2L, port.Rows[0][1]);
        Assert.Empty(_collector.Rows("c"));
    }

    [Fact]
    public async Task PreviewAsync_Destination_ReturnsItsInputWithoutExecutingIt()
    {
        // Arrange
        var definition = Define(
            [Step("s", nameof(NumbersSource), 10), Step("c", nameof(CollectDestination))],
            [Connect("s", "c")]);
        var run = CreateRun(TestContext.Current.CancellationToken);
        run.IsPreview = true;

        // Act
        var preview = await CreateExecutor().PreviewAsync(definition, "c", run);

        // Assert
        var port = Assert.Single(preview.Ports);
        Assert.Equal(DataPipelinePort.Input, port.Name);
        Assert.Equal(10, port.Rows.Count);
        Assert.Empty(_collector.Rows("c"));
    }

    private static DataPipelineDefinition Define(IEnumerable<DataPipelineStep> steps, IEnumerable<DataPipelineConnection> connections)
        => new() { Steps = [.. steps], Connections = [.. connections] };

    private DataPipelineAnalyzer CreateAnalyzer(IDataPipelineStepType[] steps = null)
        => new(new DataPipelineStepTypeManager(steps ?? TestSteps.All(_collector)), new PassThroughStringLocalizer<DataPipelineAnalyzer>());

    private DataPipelineExecutor CreateExecutor(IDataPipelineStepType[] steps = null)
    {
        var manager = new DataPipelineStepTypeManager(steps ?? TestSteps.All(_collector));

        return new DataPipelineExecutor(
            new DataPipelineAnalyzer(manager, new PassThroughStringLocalizer<DataPipelineAnalyzer>()),
            NullLogger<DataPipelineExecutor>.Instance);
    }

    private DataPipelineRunContext CreateRun(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_temp);

        return new DataPipelineRunContext
        {
            RunId = Guid.NewGuid().ToString("n"),
            PipelineId = "test",
            PipelineName = "Test",
            TemporaryDirectory = _temp,
            CancellationToken = cancellationToken,
        };
    }
}

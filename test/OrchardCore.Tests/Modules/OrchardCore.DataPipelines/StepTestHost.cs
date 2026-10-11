using System.Runtime.CompilerServices;
using System.Text.Json;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;
using OrchardCore.Entities;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

/// <summary>
/// Runs a single step against rows or files held in memory, and describes it against given input fields.
/// </summary>
internal sealed class StepTestHost : IDisposable
{
    private readonly Dictionary<string, FakeInput> _inputs = new(StringComparer.Ordinal);
    private readonly Dictionary<string, FakeOutput> _outputs = new(StringComparer.Ordinal);

    public StepTestHost(IDataPipelineStepType stepType, object settings = null, IServiceProvider services = null)
    {
        StepType = stepType;
        Step = new DataPipelineStep { StepId = "step", Type = stepType.Name };

        if (settings is not null)
        {
            Step.Properties[settings.GetType().Name] = System.Text.Json.JsonSerializer.SerializeToNode(settings, settings.GetType(), JOptions.Default);
        }

        Services = services ?? new ServiceCollection().BuildServiceProvider();
        TemporaryDirectory = Path.Combine(Path.GetTempPath(), "oc-pipeline-steps", Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(TemporaryDirectory);

        Run = new DataPipelineRunContext
        {
            RunId = "run",
            PipelineId = "pipeline",
            PipelineName = "Test pipeline",
            Services = Services,
            TemporaryDirectory = TemporaryDirectory,
            User = new ClaimsPrincipal(new ClaimsIdentity()),
            StartedUtc = new DateTime(2026, 3, 15, 14, 30, 0, DateTimeKind.Utc),
        };
    }

    public IDataPipelineStepType StepType { get; }

    public DataPipelineStep Step { get; }

    public IServiceProvider Services { get; }

    public DataPipelineRunContext Run { get; }

    public string TemporaryDirectory { get; }

    public DataPipelineStepMetrics Metrics { get; } = new();

    public List<DataPipelineLogEntry> Log { get; } = [];

    public StepTestHost WithRows(IReadOnlyList<DataField> fields, params object[][] rows)
        => WithInput(DataPipelinePort.Input, fields, rows);

    public StepTestHost WithInput(string port, IReadOnlyList<DataField> fields, params object[][] rows)
    {
        if (!_inputs.TryGetValue(port, out var input))
        {
            input = new FakeInput(new DataPipelinePort(port, port), Metrics);
            _inputs[port] = input;
        }

        input.Add(fields, rows);

        return this;
    }

    public StepTestHost WithFiles(params DataPipelineFile[] files)
    {
        var input = new FakeInput(new DataPipelinePort(DataPipelinePort.Input, "Files", DataPipelinePortKind.Files), Metrics);
        input.Files.AddRange(files);
        _inputs[DataPipelinePort.Input] = input;

        return this;
    }

    public DataPipelineFile CreateFile(string name, string content, string contentType = "text/plain")
    {
        var file = Run.CreateFile(name, contentType);
        File.WriteAllText(file.Path, content);

        return file;
    }

    public async Task<DataPipelineDescribeContext> DescribeAsync()
    {
        var inputs = _inputs.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<IReadOnlyList<DataField>>)pair.Value.FieldSets, StringComparer.Ordinal);
        var context = new DataPipelineDescribeContext(Step, inputs, Run.User, Services);

        await StepType.DescribeAsync(context);

        return context;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var description = await DescribeAsync();
        var errors = description.Issues.Where(issue => issue.Severity == DataPipelineIssueSeverity.Error).Select(issue => issue.Message).ToList();

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(" ", errors));
        }

        foreach (var port in StepType.GetOutputs(Step))
        {
            _outputs[port.Name] = new FakeOutput(port, description.Outputs.TryGetValue(port.Name, out var fields) ? fields : [], Metrics);
        }

        foreach (var port in StepType.GetInputs(Step))
        {
            if (!_inputs.ContainsKey(port.Name))
            {
                _inputs[port.Name] = new FakeInput(port, Metrics);
            }
        }

        Run.CancellationToken = cancellationToken;

        var context = new DataPipelineStepContext(
            Step,
            Run,
            _inputs.ToDictionary(pair => pair.Key, pair => (DataPipelineInput)pair.Value, StringComparer.Ordinal),
            _outputs.ToDictionary(pair => pair.Key, pair => (DataPipelineOutput)pair.Value, StringComparer.Ordinal),
            Metrics,
            Log.Add,
            cancellationToken);

        await StepType.ExecuteAsync(context);
    }

    public FakeOutput Output(string port = DataPipelinePort.Output) => _outputs[port];

    public void Dispose()
    {
        if (Directory.Exists(TemporaryDirectory))
        {
            Directory.Delete(TemporaryDirectory, recursive: true);
        }
    }

    internal sealed class FakeInput : DataPipelineInput
    {
        private readonly DataPipelineStepMetrics _metrics;
        private readonly List<DataBatch> _batches = [];

        public FakeInput(DataPipelinePort port, DataPipelineStepMetrics metrics)
        {
            Port = port;
            _metrics = metrics;
        }

        public override DataPipelinePort Port { get; }

        private readonly List<IReadOnlyList<DataField>> _fieldSets = [];

        public override bool IsConnected => _fieldSets.Count > 0 || Files.Count > 0;

        public override IReadOnlyList<DataField> Fields => _fieldSets.Count > 0 ? _fieldSets[0] : [];

        public override IReadOnlyList<IReadOnlyList<DataField>> FieldSets => _fieldSets;

        public List<DataPipelineFile> Files { get; } = [];

        public void Add(IReadOnlyList<DataField> fields, object[][] rows)
        {
            _fieldSets.Add(fields);

            // Two rows per batch, to exercise steps across batches.
            foreach (var chunk in rows.Chunk(2))
            {
                _batches.Add(new DataBatch(fields, chunk));
            }
        }

        public override async IAsyncEnumerable<DataBatch> ReadBatchesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var batch in _batches)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
                _metrics.AddRowsIn(batch.Count);

                yield return batch;
            }
        }

        public override async IAsyncEnumerable<DataPipelineFile> ReadFilesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var file in Files)
            {
                await Task.Yield();
                _metrics.AddFileIn();

                yield return file;
            }
        }
    }

    internal sealed class FakeOutput : DataPipelineOutput
    {
        private readonly DataPipelineStepMetrics _metrics;

        public FakeOutput(DataPipelinePort port, IReadOnlyList<DataField> fields, DataPipelineStepMetrics metrics)
        {
            Port = port;
            Fields = fields;
            _metrics = metrics;
        }

        public override DataPipelinePort Port { get; }

        public override bool IsConnected => true;

        public override bool IsClosed => false;

        public override IReadOnlyList<DataField> Fields { get; }

        public List<DataBatch> Batches { get; } = [];

        public List<DataPipelineFile> Files { get; } = [];

        public List<object[]> Rows => Batches.SelectMany(batch => batch.Rows).ToList();

        public IReadOnlyList<DataField> RuntimeFields => Batches.Count > 0 ? Batches[0].Fields : Fields;

        public object Value(int row, string field) => Rows[row][RuntimeFields.IndexOf(field)];

        public override ValueTask WriteAsync(DataBatch batch, CancellationToken cancellationToken = default)
        {
            _metrics.AddRowsOut(batch.Count);
            Batches.Add(batch);

            return ValueTask.CompletedTask;
        }

        public override ValueTask WriteFileAsync(DataPipelineFile file, CancellationToken cancellationToken = default)
        {
            _metrics.AddFileOut();
            Files.Add(file);

            return ValueTask.CompletedTask;
        }
    }
}

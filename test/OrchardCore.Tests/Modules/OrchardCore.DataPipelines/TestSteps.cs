using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;
using OrchardCore.Entities;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

/// <summary>
/// Step types for engine tests. Their settings are kept in the step properties under simple names.
/// </summary>
internal static class TestSteps
{
    public static readonly DataField[] NumberFields = [new("Id", "Id", DataFieldType.Integer)];

    public static DataPipelineStep Step(string id, string type, int count = 0, string message = null)
    {
        var step = new DataPipelineStep { StepId = id, Type = type };
        step.Put(new TestSettings { Count = count, Message = message });

        return step;
    }

    public static DataPipelineConnection Connect(string source, string target, string sourcePort = DataPipelinePort.Output, string targetPort = DataPipelinePort.Input)
        => new() { SourceStepId = source, SourcePort = sourcePort, TargetStepId = target, TargetPort = targetPort };

    public static IDataPipelineStepType[] All(TestCollector collector)
        =>
        [
            new NumbersSource(),
            new DoubleTransform(),
            new LimitTransform(),
            new FailTransform(),
            new UnionTransform(),
            new CollectDestination(collector),
            new FileMaker(),
            new FileCollector(collector),
        ];

    public sealed class TestSettings
    {
        public int Count { get; set; }

        public string Message { get; set; }
    }

    public abstract class TestStep : DataPipelineStepType<TestSettings>
    {
        public override string Name => GetType().Name;

        public override LocalizedString DisplayName => new(Name, Name);

        public override LocalizedString Description => new(Name, Name);
    }

    /// <summary>
    /// Writes the numbers 1 to Count, in batches of 100, or a preview's limit.
    /// </summary>
    public sealed class NumbersSource : TestStep
    {
        public int Executions;

        public override DataPipelineStepCategory Category => DataPipelineStepCategory.Source;

        public override Task DescribeAsync(DataPipelineDescribeContext context)
        {
            context.SetOutputFields(NumberFields);

            return Task.CompletedTask;
        }

        public override async Task ExecuteAsync(DataPipelineStepContext context)
        {
            Interlocked.Increment(ref Executions);

            var count = GetSettings(context.Step).Count;

            if (context.Run.IsPreview)
            {
                count = Math.Min(count, context.Run.PreviewRowLimit);
            }

            var output = context.GetOutput();

            foreach (var chunk in Enumerable.Range(1, count).Chunk(100))
            {
                if (output.IsClosed)
                {
                    return;
                }

                await output.WriteAsync(new DataBatch(NumberFields, chunk.Select(number => new object[] { (long)number }).ToList()), context.CancellationToken);
            }
        }
    }

    /// <summary>
    /// Adds a Double field.
    /// </summary>
    public sealed class DoubleTransform : TestStep
    {
        public override DataPipelineStepCategory Category => DataPipelineStepCategory.Transform;

        public override Task DescribeAsync(DataPipelineDescribeContext context)
        {
            var input = context.GetInputFields();

            if (input.IndexOf("Id") < 0)
            {
                context.AddError("The input has no Id field.");
            }

            context.SetOutputFields([.. input, new DataField("Double", "Double", DataFieldType.Integer)]);

            return Task.CompletedTask;
        }

        public override async Task ExecuteAsync(DataPipelineStepContext context)
        {
            var output = context.GetOutput();
            var fields = output.Fields;

            await foreach (var batch in context.GetInput().ReadBatchesAsync(context.CancellationToken))
            {
                var index = batch.IndexOf("Id");
                var rows = batch.Rows.Select(row => (object[])[.. row, (long)row[index] * 2]).ToList();
                await output.WriteAsync(new DataBatch(fields, rows), context.CancellationToken);
            }
        }
    }

    /// <summary>
    /// Keeps the first Count rows, then stops reading.
    /// </summary>
    public sealed class LimitTransform : TestStep
    {
        public override DataPipelineStepCategory Category => DataPipelineStepCategory.Transform;

        public override async Task ExecuteAsync(DataPipelineStepContext context)
        {
            var remaining = GetSettings(context.Step).Count;
            var output = context.GetOutput();

            await foreach (var batch in context.GetInput().ReadBatchesAsync(context.CancellationToken))
            {
                var rows = batch.Rows.Take(remaining).ToList();
                remaining -= rows.Count;
                await output.WriteAsync(new DataBatch(batch.Fields, rows), context.CancellationToken);

                if (remaining <= 0)
                {
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Throws once it has read Count rows.
    /// </summary>
    public sealed class FailTransform : TestStep
    {
        public override DataPipelineStepCategory Category => DataPipelineStepCategory.Transform;

        public override async Task ExecuteAsync(DataPipelineStepContext context)
        {
            var settings = GetSettings(context.Step);
            var read = 0;

            await foreach (var batch in context.GetInput().ReadBatchesAsync(context.CancellationToken))
            {
                read += batch.Count;

                if (read >= settings.Count)
                {
                    throw new InvalidOperationException(settings.Message ?? "Boom");
                }

                await context.GetOutput().WriteAsync(batch, context.CancellationToken);
            }
        }
    }

    /// <summary>
    /// Appends every connection of its input.
    /// </summary>
    public sealed class UnionTransform : TestStep
    {
        public override DataPipelineStepCategory Category => DataPipelineStepCategory.Transform;

        public override IReadOnlyList<DataPipelinePort> GetInputs(DataPipelineStep step)
            => [new(DataPipelinePort.Input, "Rows") { IsRequired = true, AllowsMany = true }];

        public override async Task ExecuteAsync(DataPipelineStepContext context)
        {
            await foreach (var batch in context.GetInput().ReadBatchesAsync(context.CancellationToken))
            {
                await context.GetOutput().WriteAsync(batch, context.CancellationToken);
            }
        }
    }

    /// <summary>
    /// Collects the rows it reads, keyed by its step id.
    /// </summary>
    public sealed class CollectDestination : TestStep
    {
        private readonly TestCollector _collector;

        public CollectDestination(TestCollector collector)
        {
            _collector = collector;
        }

        public override DataPipelineStepCategory Category => DataPipelineStepCategory.Destination;

        public override IReadOnlyList<DataPipelinePort> GetInputs(DataPipelineStep step)
            => [new(DataPipelinePort.Input, "Rows") { IsRequired = true }];

        public override async Task ExecuteAsync(DataPipelineStepContext context)
        {
            var delay = GetSettings(context.Step).Count;

            await foreach (var batch in context.GetInput().ReadBatchesAsync(context.CancellationToken))
            {
                if (delay > 0)
                {
                    await Task.Delay(delay, context.CancellationToken);
                }

                _collector.Add(context.Step.StepId, batch.Rows);
            }

            context.AddDelivery($"Collected by {context.Step.StepId}");
        }
    }

    /// <summary>
    /// Writes its rows to a text file, one number per line.
    /// </summary>
    public sealed class FileMaker : TestStep
    {
        public override DataPipelineStepCategory Category => DataPipelineStepCategory.File;

        public override async Task ExecuteAsync(DataPipelineStepContext context)
        {
            var file = context.Run.CreateFile("numbers.txt", "text/plain");
            var count = 0L;

            await using (var writer = new StreamWriter(file.Path))
            {
                await foreach (var batch in context.GetInput().ReadBatchesAsync(context.CancellationToken))
                {
                    foreach (var row in batch.Rows)
                    {
                        await writer.WriteLineAsync(row[0].ToString());
                        count++;
                    }
                }
            }

            file.RowCount = count;
            await context.GetOutput().WriteFileAsync(file, context.CancellationToken);
        }
    }

    /// <summary>
    /// Collects the content of the files it reads.
    /// </summary>
    public sealed class FileCollector : TestStep
    {
        private readonly TestCollector _collector;

        public FileCollector(TestCollector collector)
        {
            _collector = collector;
        }

        public override DataPipelineStepCategory Category => DataPipelineStepCategory.Destination;

        public override async Task ExecuteAsync(DataPipelineStepContext context)
        {
            await foreach (var file in context.GetInput().ReadFilesAsync(context.CancellationToken))
            {
                _collector.AddFile(file.FileName, await File.ReadAllTextAsync(file.Path));
            }
        }
    }
}

internal sealed class TestCollector
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<object[]>> _rows = new();

    public ConcurrentDictionary<string, string> Files { get; } = new();

    public void Add(string stepId, IEnumerable<object[]> rows)
    {
        var queue = _rows.GetOrAdd(stepId, _ => new ConcurrentQueue<object[]>());

        foreach (var row in rows)
        {
            queue.Enqueue(row);
        }
    }

    public void AddFile(string name, string content) => Files[name] = content;

    public IReadOnlyList<object[]> Rows(string stepId) => _rows.TryGetValue(stepId, out var queue) ? queue.ToList() : [];
}

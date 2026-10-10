using Microsoft.Extensions.Logging;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Runs a pipeline. Every step runs at the same time, and batches of rows and files flow between them through
/// channels: a step starts working on the first batch while the step before it reads the next one, and a slow step
/// makes the faster ones wait, so the memory used doesn't depend on the number of rows. A step that fails cancels the
/// run.
/// </summary>
public sealed class DataPipelineExecutor
{
    private readonly DataPipelineAnalyzer _analyzer;
    private readonly ILogger _logger;

    public DataPipelineExecutor(
        DataPipelineAnalyzer analyzer,
        ILogger<DataPipelineExecutor> logger)
    {
        _analyzer = analyzer;
        _logger = logger;
    }

    /// <summary>
    /// Runs a pipeline.
    /// </summary>
    /// <param name="definition">The pipeline.</param>
    /// <param name="run">The run.</param>
    /// <param name="observer">An optional observer of the run's progress.</param>
    /// <returns>The outcome of the run.</returns>
    public async Task<DataPipelineExecutionResult> ExecuteAsync(
        DataPipelineDefinition definition,
        DataPipelineRunContext run,
        IDataPipelineRunObserver observer = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(run);

        var analysis = await _analyzer.AnalyzeAsync(definition, run.User, run.Services, run.CancellationToken);

        if (analysis.HasErrors)
        {
            var error = analysis.Issues.First(issue => issue.Severity == DataPipelineIssueSeverity.Error);

            return new DataPipelineExecutionResult
            {
                Status = DataPipelineRunStatus.Failed,
                Error = error.Message,
                FailedStepId = error.StepId,
                Issues = analysis.Issues,
            };
        }

        var graph = new RunGraph(analysis, analysis.Order);
        var result = await RunAsync(graph, run, observer, captures: []);
        result.Issues = analysis.Issues;

        return result;
    }

    /// <summary>
    /// Previews a step: runs the steps it reads from, reading a few rows from each source, and returns the first rows
    /// the step produces. For a destination, which a preview never executes, it returns what the destination would
    /// receive.
    /// </summary>
    /// <param name="definition">The pipeline.</param>
    /// <param name="stepId">The step to preview.</param>
    /// <param name="run">The run. It is marked as a preview.</param>
    /// <returns>The preview.</returns>
    public async Task<DataPipelinePreview> PreviewAsync(DataPipelineDefinition definition, string stepId, DataPipelineRunContext run)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(run);

        run.IsPreview = true;

        var preview = new DataPipelinePreview { StepId = stepId };
        var analysis = await _analyzer.AnalyzeAsync(definition, run.User, run.Services, run.CancellationToken);
        preview.Issues = analysis.Issues;

        var target = analysis.GetStep(stepId);

        if (target?.StepType is null)
        {
            preview.Error = "The step can't be previewed because its type is not available.";

            return preview;
        }

        var ancestors = Ancestors(analysis, stepId);
        var isDestination = target.StepType.Category == DataPipelineStepCategory.Destination;
        var scope = isDestination ? ancestors : [.. ancestors, stepId];

        if (analysis.Issues.Any(issue => issue.Severity == DataPipelineIssueSeverity.Error && (issue.StepId is null || scope.Contains(issue.StepId) || issue.StepId == stepId)))
        {
            preview.Error = "Fix the errors of this step and of the steps before it to preview it.";

            return preview;
        }

        if (ancestors.Any(id => analysis.GetStep(id).StepType.Category == DataPipelineStepCategory.Destination))
        {
            preview.Error = "A step that comes after a destination can't be previewed, because a preview never executes destinations.";

            return preview;
        }

        var captures = new List<PreviewCapture>();

        if (isDestination)
        {
            foreach (var port in target.InputPorts)
            {
                captures.Add(new PreviewCapture(port, target.GetInputFields(port.Name), target.IncomingConnections.TryGetValue(port.Name, out var connections) ? connections : [], null, run.PreviewRowLimit));
            }
        }
        else
        {
            foreach (var port in target.OutputPorts)
            {
                captures.Add(new PreviewCapture(port, target.Outputs.TryGetValue(port.Name, out var fields) ? fields : [], [], stepId, run.PreviewRowLimit));
            }
        }

        var order = analysis.Order.Where(scope.Contains).ToList();
        var graph = new RunGraph(analysis, order);
        var result = await RunAsync(graph, run, observer: null, captures);

        if (result.Status == DataPipelineRunStatus.Failed)
        {
            preview.Error = result.Error;
        }

        foreach (var capture in captures)
        {
            preview.Ports.Add(capture.ToPreviewPort());
        }

        return preview;
    }

    private async Task<DataPipelineExecutionResult> RunAsync(
        RunGraph graph,
        DataPipelineRunContext run,
        IDataPipelineRunObserver observer,
        List<PreviewCapture> captures)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(run.CancellationToken);
        var token = cancellation.Token;

        var results = graph.Order.ToDictionary(id => id, _ => new DataPipelineStepResult { Status = DataPipelineStepStatus.Pending }, StringComparer.Ordinal);
        var failures = new List<(DateTime Utc, string StepId, Exception Error)>();
        var failuresLock = new object();

        // Wire the edges of the steps that run, and of the previewed ports.
        var edges = new Dictionary<DataPipelineConnection, DataPipelineEdge>();

        foreach (var stepId in graph.Order)
        {
            var analysis = graph.Analysis.GetStep(stepId);
            var unbounded = analysis.IncomingConnections.Values.Sum(connections => connections.Count) > 1;

            foreach (var connection in analysis.IncomingConnections.Values.SelectMany(connections => connections))
            {
                if (graph.Contains(connection.SourceStepId))
                {
                    edges[connection] = new DataPipelineEdge(unbounded);
                }
            }
        }

        var captureEdges = new Dictionary<(string StepId, string Port), List<DataPipelineEdge>>();

        foreach (var capture in captures)
        {
            if (capture.SourceStepId is not null)
            {
                // The outputs of the previewed step.
                var edge = new DataPipelineEdge(unbounded: true);
                capture.Edges.Add(edge);
                AddEdge(captureEdges, (capture.SourceStepId, capture.Port.Name), edge);
            }
            else
            {
                // The inputs of a previewed destination.
                foreach (var connection in capture.Connections)
                {
                    var edge = new DataPipelineEdge(unbounded: true);
                    capture.Edges.Add(edge);
                    AddEdge(captureEdges, (connection.SourceStepId, connection.SourcePort), edge);
                }
            }
        }

        var tasks = new List<Task>();

        foreach (var stepId in graph.Order)
        {
            var analysis = graph.Analysis.GetStep(stepId);
            var metrics = results[stepId].Metrics;

            var inputs = new Dictionary<string, DataPipelineChannelInput>(StringComparer.Ordinal);

            foreach (var port in analysis.InputPorts)
            {
                var connections = analysis.IncomingConnections.TryGetValue(port.Name, out var list) ? list : [];
                var portEdges = connections.Where(edges.ContainsKey).Select(connection => edges[connection]).ToList();
                var fieldSets = analysis.Inputs.TryGetValue(port.Name, out var sets) ? sets : [];

                inputs[port.Name] = new DataPipelineChannelInput(port, fieldSets, portEdges, metrics);
            }

            var outputs = new Dictionary<string, DataPipelineChannelOutput>(StringComparer.Ordinal);

            foreach (var port in analysis.OutputPorts)
            {
                var connections = analysis.OutgoingConnections.TryGetValue(port.Name, out var list) ? list : [];
                var portEdges = connections.Where(edges.ContainsKey).Select(connection => edges[connection]).ToList();

                if (captureEdges.TryGetValue((stepId, port.Name), out var extra))
                {
                    portEdges.AddRange(extra);
                }

                var fields = analysis.Outputs.TryGetValue(port.Name, out var described) ? described : [];

                outputs[port.Name] = new DataPipelineChannelOutput(port, fields, portEdges, metrics);
            }

            var inputViews = inputs.ToDictionary(pair => pair.Key, pair => (DataPipelineInput)pair.Value, StringComparer.Ordinal);
            var outputViews = outputs.ToDictionary(pair => pair.Key, pair => (DataPipelineOutput)pair.Value, StringComparer.Ordinal);

            DataPipelineStepContext CreateContext(IServiceProvider services)
                => new(analysis.Step, run, inputViews, outputViews, metrics, entry => observer?.Log(entry), token, services);

            tasks.Add(Task.Run(() => RunStepAsync(analysis, CreateContext, inputs.Values, outputs.Values, results[stepId], cancellation, observer, failures, failuresLock), CancellationToken.None));
        }

        foreach (var capture in captures)
        {
            tasks.Add(Task.Run(() => capture.ReadAsync(token), CancellationToken.None));
        }

        await Task.WhenAll(tasks);

        var result = new DataPipelineExecutionResult
        {
            Steps = results,
            Deliveries = [.. run.Deliveries],
        };

        if (failures.Count > 0)
        {
            var first = failures.OrderBy(failure => failure.Utc).First();
            result.Status = DataPipelineRunStatus.Failed;
            result.FailedStepId = first.StepId;
            result.Error = first.Error.Message;
        }
        else if (run.CancellationToken.IsCancellationRequested)
        {
            result.Status = DataPipelineRunStatus.Cancelled;
            result.Error = "The run was cancelled.";
        }
        else
        {
            result.Status = DataPipelineRunStatus.Succeeded;
        }

        return result;
    }

    private async Task RunStepAsync(
        DataPipelineStepAnalysis analysis,
        Func<IServiceProvider, DataPipelineStepContext> createContext,
        IEnumerable<DataPipelineChannelInput> inputs,
        IEnumerable<DataPipelineChannelOutput> outputs,
        DataPipelineStepResult result,
        CancellationTokenSource cancellation,
        IDataPipelineRunObserver observer,
        List<(DateTime Utc, string StepId, Exception Error)> failures,
        object failuresLock)
    {
        var step = analysis.Step;
        var run = createContext(null).Run;
        var token = cancellation.Token;
        Exception error = null;

        result.Status = DataPipelineStepStatus.Running;
        observer?.StepStarted(step, result.Metrics);

        try
        {
            token.ThrowIfCancellationRequested();

            if (run.StepScope is null)
            {
                await analysis.StepType.ExecuteAsync(createContext(null));
            }
            else
            {
                await run.StepScope(step, services => analysis.StepType.ExecuteAsync(createContext(services)));
            }

            token.ThrowIfCancellationRequested();

            result.Status = DataPipelineStepStatus.Succeeded;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            result.Status = DataPipelineStepStatus.Cancelled;
        }
        catch (Exception ex)
        {
            error = ex;
            result.Status = DataPipelineStepStatus.Failed;
            result.Error = ex.Message;

            lock (failuresLock)
            {
                failures.Add((DateTime.UtcNow, step.StepId, ex));
            }

            _logger.LogWarning(ex, "The step '{StepId}' of the data pipeline run '{RunId}' failed.", step.StepId, run.RunId);

            // Stop the other steps before closing the outputs, so the steps downstream don't take what they read
            // so far for a complete input.
            await cancellation.CancelAsync();
        }
        finally
        {
            foreach (var output in outputs)
            {
                output.Complete(result.Status == DataPipelineStepStatus.Succeeded ? null : new OperationCanceledException());
            }

            foreach (var input in inputs)
            {
                input.Complete();
            }

            observer?.StepCompleted(step, result.Status, error);
        }
    }

    private static HashSet<string> Ancestors(DataPipelineAnalysis analysis, string stepId)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>([stepId]);

        while (pending.Count > 0)
        {
            var current = analysis.GetStep(pending.Pop());

            if (current is null)
            {
                continue;
            }

            foreach (var connection in current.IncomingConnections.Values.SelectMany(connections => connections))
            {
                if (result.Add(connection.SourceStepId))
                {
                    pending.Push(connection.SourceStepId);
                }
            }
        }

        return result;
    }

    private static void AddEdge(Dictionary<(string StepId, string Port), List<DataPipelineEdge>> map, (string StepId, string Port) key, DataPipelineEdge edge)
    {
        if (!map.TryGetValue(key, out var list))
        {
            list = [];
            map[key] = list;
        }

        list.Add(edge);
    }

    private sealed class RunGraph
    {
        private readonly HashSet<string> _steps;

        public RunGraph(DataPipelineAnalysis analysis, IReadOnlyList<string> order)
        {
            Analysis = analysis;
            Order = order;
            _steps = new HashSet<string>(order, StringComparer.Ordinal);
        }

        public DataPipelineAnalysis Analysis { get; }

        public IReadOnlyList<string> Order { get; }

        public bool Contains(string stepId) => stepId is not null && _steps.Contains(stepId);
    }

    private sealed class PreviewCapture
    {
        private readonly int _limit;

        public PreviewCapture(DataPipelinePort port, IReadOnlyList<DataField> fields, IReadOnlyList<DataPipelineConnection> connections, string sourceStepId, int limit)
        {
            Port = port;
            Fields = fields ?? [];
            Connections = connections;
            SourceStepId = sourceStepId;
            _limit = Math.Max(1, limit);
        }

        public DataPipelinePort Port { get; }

        public IReadOnlyList<DataField> Fields { get; }

        public IReadOnlyList<DataPipelineConnection> Connections { get; }

        public string SourceStepId { get; }

        public List<DataPipelineEdge> Edges { get; } = [];

        public List<object[]> Rows { get; } = [];

        public List<DataPipelinePreviewFile> Files { get; } = [];

        public bool Truncated { get; private set; }

        public IReadOnlyList<DataField> RuntimeFields { get; private set; }

        public async Task ReadAsync(CancellationToken cancellationToken)
        {
            try
            {
                foreach (var edge in Edges)
                {
                    await foreach (var item in edge.Channel.Reader.ReadAllAsync(cancellationToken))
                    {
                        if (item is DataBatch batch)
                        {
                            RuntimeFields ??= batch.Fields;

                            foreach (var row in batch.Rows)
                            {
                                if (Rows.Count == _limit)
                                {
                                    Truncated = true;
                                    edge.CompleteConsumer();

                                    break;
                                }

                                Rows.Add(row);
                            }
                        }
                        else if (item is DataPipelineFile file)
                        {
                            Files.Add(new DataPipelinePreviewFile
                            {
                                FileName = file.FileName,
                                ContentType = file.ContentType,
                                Length = file.Length,
                                RowCount = file.RowCount,
                            });
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // The run failed or was cancelled; keep what was captured.
            }
            finally
            {
                foreach (var edge in Edges)
                {
                    edge.CompleteConsumer();
                }
            }
        }

        public DataPipelinePreviewPort ToPreviewPort()
            => new()
            {
                Name = Port.Name,
                DisplayName = Port.DisplayName,
                Kind = Port.Kind,
                Fields = RuntimeFields ?? Fields,
                Rows = Rows,
                Truncated = Truncated,
                Files = Files,
            };
    }
}

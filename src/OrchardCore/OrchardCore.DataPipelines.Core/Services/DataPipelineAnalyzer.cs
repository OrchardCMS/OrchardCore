using System.Security.Claims;
using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Checks a pipeline before it is published or run: every step type must be available, every connection must join an
/// output to an input of the same kind, required inputs must be connected, the steps must not form a loop, and each
/// step must accept the fields of its inputs. It also resolves the fields that flow through each step, which the
/// designer lists for the steps that come next.
/// </summary>
public sealed class DataPipelineAnalyzer
{
    private readonly IDataPipelineStepTypeManager _stepTypeManager;
    private readonly IStringLocalizer S;

    public DataPipelineAnalyzer(IDataPipelineStepTypeManager stepTypeManager, IStringLocalizer<DataPipelineAnalyzer> localizer)
    {
        _stepTypeManager = stepTypeManager;
        S = localizer;
    }

    /// <summary>
    /// Analyzes a pipeline.
    /// </summary>
    /// <param name="definition">The pipeline.</param>
    /// <param name="user">The user the pipeline is designed or run for.</param>
    /// <param name="services">The services of the current scope.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The analysis.</returns>
    public async Task<DataPipelineAnalysis> AnalyzeAsync(
        DataPipelineDefinition definition,
        ClaimsPrincipal user,
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var issues = new List<DataPipelineIssue>();
        var steps = new Dictionary<string, DataPipelineStepAnalysis>(StringComparer.Ordinal);

        if (definition.Steps.Count == 0)
        {
            issues.Add(new(DataPipelineIssueSeverity.Error, S["The pipeline has no steps. Add a source to read data from."]));
        }

        foreach (var step in definition.Steps)
        {
            if (string.IsNullOrEmpty(step.StepId) || steps.ContainsKey(step.StepId))
            {
                issues.Add(new(DataPipelineIssueSeverity.Error, S["Two steps have the same identifier."], step.StepId));

                continue;
            }

            var stepType = _stepTypeManager.GetStepType(step.Type);

            if (stepType is null)
            {
                issues.Add(new(DataPipelineIssueSeverity.Error, S["The step type '{0}' is not available. The feature that provides it may be disabled.", step.Type ?? string.Empty], step.StepId));
            }

            steps[step.StepId] = new DataPipelineStepAnalysis(step, stepType)
            {
                InputPorts = stepType?.GetInputs(step) ?? [],
                OutputPorts = stepType?.GetOutputs(step) ?? [],
            };
        }

        var incoming = steps.Keys.ToDictionary(id => id, _ => new Dictionary<string, List<DataPipelineConnection>>(StringComparer.Ordinal), StringComparer.Ordinal);
        var outgoing = steps.Keys.ToDictionary(id => id, _ => new Dictionary<string, List<DataPipelineConnection>>(StringComparer.Ordinal), StringComparer.Ordinal);
        var edges = new List<DataPipelineConnection>();

        foreach (var connection in definition.Connections)
        {
            if (!ValidateConnection(connection, steps, issues))
            {
                continue;
            }

            edges.Add(connection);
            Add(incoming[connection.TargetStepId], connection.TargetPort, connection);
            Add(outgoing[connection.SourceStepId], connection.SourcePort, connection);
        }

        foreach (var analysis in steps.Values)
        {
            var stepId = analysis.Step.StepId;

            foreach (var port in analysis.InputPorts)
            {
                var count = incoming[stepId].TryGetValue(port.Name, out var connections) ? connections.Count : 0;

                if (count == 0 && port.IsRequired)
                {
                    issues.Add(new(DataPipelineIssueSeverity.Error, S["Connect the '{0}' input of this step.", port.DisplayName], stepId));
                }
                else if (count > 1 && !port.AllowsMany)
                {
                    issues.Add(new(DataPipelineIssueSeverity.Error, S["The '{0}' input of this step accepts one connection only.", port.DisplayName], stepId));
                }
            }

            if (analysis.StepType is not null &&
                analysis.StepType.Category != DataPipelineStepCategory.Destination &&
                analysis.OutputPorts.Count > 0 &&
                outgoing[stepId].Count == 0)
            {
                issues.Add(new(DataPipelineIssueSeverity.Warning, S["Nothing uses the output of this step. Connect it to a next step, or remove it."], stepId));
            }

            analysis.IncomingConnections = incoming[stepId].ToDictionary(pair => pair.Key, pair => (IReadOnlyList<DataPipelineConnection>)pair.Value, StringComparer.Ordinal);
            analysis.OutgoingConnections = outgoing[stepId].ToDictionary(pair => pair.Key, pair => (IReadOnlyList<DataPipelineConnection>)pair.Value, StringComparer.Ordinal);
        }

        var order = Sort(definition, steps, edges, issues);

        await DescribeAsync(order, steps, issues, user, services, cancellationToken);

        return new DataPipelineAnalysis(issues, order, steps);
    }

    private bool ValidateConnection(DataPipelineConnection connection, Dictionary<string, DataPipelineStepAnalysis> steps, List<DataPipelineIssue> issues)
    {
        if (connection is null ||
            !steps.TryGetValue(connection.SourceStepId ?? string.Empty, out var source) ||
            !steps.TryGetValue(connection.TargetStepId ?? string.Empty, out var target))
        {
            issues.Add(new(DataPipelineIssueSeverity.Error, S["A connection refers to a step that doesn't exist."], connection?.TargetStepId));

            return false;
        }

        if (source.StepType is null || target.StepType is null)
        {
            // The missing step type is already reported.
            return false;
        }

        if (connection.SourceStepId == connection.TargetStepId)
        {
            issues.Add(new(DataPipelineIssueSeverity.Error, S["A step can't be connected to itself."], connection.TargetStepId));

            return false;
        }

        var output = source.OutputPorts.FirstOrDefault(port => port.Name == connection.SourcePort);
        var input = target.InputPorts.FirstOrDefault(port => port.Name == connection.TargetPort);

        if (output is null || input is null)
        {
            issues.Add(new(DataPipelineIssueSeverity.Error, S["A connection uses an input or output this step doesn't have."], output is null ? connection.SourceStepId : connection.TargetStepId));

            return false;
        }

        if (output.Kind != input.Kind)
        {
            issues.Add(new(
                DataPipelineIssueSeverity.Error,
                output.Kind == DataPipelinePortKind.Records
                    ? S["This step expects files, but it is connected to rows. Add a step that creates a file first."]
                    : S["This step expects rows, but it is connected to files."],
                connection.TargetStepId));

            return false;
        }

        return true;
    }

    private List<string> Sort(DataPipelineDefinition definition, Dictionary<string, DataPipelineStepAnalysis> steps, List<DataPipelineConnection> edges, List<DataPipelineIssue> issues)
    {
        var indegree = steps.Keys.ToDictionary(id => id, _ => 0, StringComparer.Ordinal);

        foreach (var edge in edges)
        {
            indegree[edge.TargetStepId]++;
        }

        // Keep the order of the definition among the steps that are ready, so the order is stable.
        var ready = new Queue<string>(definition.Steps.Select(step => step.StepId).Where(id => id is not null && indegree.TryGetValue(id, out var count) && count == 0).Distinct());
        var order = new List<string>();

        while (ready.Count > 0)
        {
            var stepId = ready.Dequeue();
            order.Add(stepId);

            foreach (var edge in edges.Where(edge => edge.SourceStepId == stepId))
            {
                if (--indegree[edge.TargetStepId] == 0)
                {
                    ready.Enqueue(edge.TargetStepId);
                }
            }
        }

        foreach (var (stepId, count) in indegree)
        {
            if (count > 0)
            {
                issues.Add(new(DataPipelineIssueSeverity.Error, S["This step is part of a loop. The data of a step can't flow back into it."], stepId));
            }
        }

        return order;
    }

    private async Task DescribeAsync(
        List<string> order,
        Dictionary<string, DataPipelineStepAnalysis> steps,
        List<DataPipelineIssue> issues,
        ClaimsPrincipal user,
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        foreach (var stepId in order)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var analysis = steps[stepId];

            if (analysis.StepType is null)
            {
                continue;
            }

            var inputs = new Dictionary<string, IReadOnlyList<IReadOnlyList<DataField>>>(StringComparer.Ordinal);
            var upstreamDescribed = true;

            foreach (var (port, connections) in analysis.IncomingConnections)
            {
                var sets = new List<IReadOnlyList<DataField>>();

                foreach (var connection in connections)
                {
                    var source = steps[connection.SourceStepId];

                    if (!source.IsDescribed)
                    {
                        upstreamDescribed = false;

                        continue;
                    }

                    sets.Add(source.Outputs.TryGetValue(connection.SourcePort, out var fields) ? fields : []);
                }

                inputs[port] = sets;
            }

            analysis.Inputs = inputs;

            // A step that reads from a step with errors is not described: its own issues would only repeat them.
            if (!upstreamDescribed)
            {
                continue;
            }

            var context = new DataPipelineDescribeContext(analysis.Step, inputs, user, services, cancellationToken);

            try
            {
                await analysis.StepType.DescribeAsync(context);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                context.AddError(S["This step couldn't be checked: {0}", ex.Message]);
            }

            issues.AddRange(context.Issues);

            analysis.Outputs = context.Outputs.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
            analysis.IsDescribed = !context.Issues.Any(issue => issue.Severity == DataPipelineIssueSeverity.Error);
        }
    }

    private static void Add(Dictionary<string, List<DataPipelineConnection>> map, string port, DataPipelineConnection connection)
    {
        if (!map.TryGetValue(port, out var list))
        {
            list = [];
            map[port] = list;
        }

        list.Add(connection);
    }
}

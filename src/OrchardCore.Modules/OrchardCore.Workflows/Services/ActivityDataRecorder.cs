using System.Text.Json.Nodes;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// Collects the data of an activity's execution for its journal record, when its workflow records activity data
/// (<see cref="WorkflowType.RecordActivityData"/>): the evaluations and outputs the activity reported, the variables
/// and properties it changed, and its last result.
/// </summary>
internal sealed class ActivityDataRecorder
{
    private const string Ellipsis = "…";

    private readonly WorkflowExecutionContext _workflowContext;
    private readonly ActivityContext _activityContext;
    private readonly Dictionary<string, string> _propertiesBefore;
    private readonly object _lastResultBefore;

    private ActivityDataRecorder(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        _workflowContext = workflowContext;
        _activityContext = activityContext;

        // What was reported before the activity runs isn't its own.
        workflowContext.TakeReportedData();
        _propertiesBefore = Snapshot(workflowContext);
        _lastResultBefore = workflowContext.LastResult;
    }

    /// <summary>
    /// Starts recording the execution of an activity, or returns <see langword="null"/> when its workflow doesn't
    /// record activity data.
    /// </summary>
    public static ActivityDataRecorder Start(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => workflowContext.RecordsActivityData ? new ActivityDataRecorder(workflowContext, activityContext) : null;

    /// <summary>
    /// Returns what the activity evaluated, set and changed since <see cref="Start"/>, within
    /// <see cref="WorkflowExecutionData.MaxDataLength"/>.
    /// </summary>
    public WorkflowExecutionData Collect()
    {
        var data = _workflowContext.TakeReportedData();
        var properties = _activityContext.Activity?.Properties;

        foreach (var evaluation in data.Evaluations)
        {
            evaluation.Property = FindExpressionProperty(properties, evaluation.Expression, null);
        }

        // The declared variables are listed apart from the other properties, which activities such as Set Property write.
        var declared = new HashSet<string>(
            (_workflowContext.WorkflowType?.Variables ?? []).Select(variable => variable.Name).Where(name => !string.IsNullOrEmpty(name)),
            StringComparer.OrdinalIgnoreCase);

        foreach (var (name, value) in _workflowContext.Properties)
        {
            var text = WorkflowExecutionData.FormatValue(value, out var truncated);

            if (_propertiesBefore.TryGetValue(name, out var before) && before == text)
            {
                continue;
            }

            (declared.Contains(name) ? data.Variables : data.Properties)[name] = text;
            data.IsTruncated |= truncated;
        }

        if (!ReferenceEquals(_lastResultBefore, _workflowContext.LastResult))
        {
            data.LastResult = WorkflowExecutionData.FormatValue(_workflowContext.LastResult, out var truncated);
            data.IsTruncated |= truncated;
        }

        KeepWithinBudget(data);

        return data;
    }

    private static Dictionary<string, string> Snapshot(WorkflowExecutionContext workflowContext)
    {
        var snapshot = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (name, value) in workflowContext.Properties)
        {
            snapshot[name] = WorkflowExecutionData.FormatValue(value, out _);
        }

        return snapshot;
    }

    /// <summary>
    /// Returns the path of the activity property whose expression is <paramref name="expression"/>, such as
    /// <c>Condition</c> or <c>Inputs.amount</c>, or <see langword="null"/>.
    /// </summary>
    internal static string FindExpressionProperty(JsonNode node, string expression, string path)
    {
        if (node is JsonObject jsonObject)
        {
            if (path is not null &&
                jsonObject.TryGetPropertyValue(nameof(WorkflowExpression<object>.Expression), out var text) &&
                text is JsonValue value &&
                value.TryGetValue<string>(out var candidate) &&
                Matches(candidate, expression))
            {
                return path;
            }

            foreach (var (key, child) in jsonObject)
            {
                if (FindExpressionProperty(child, expression, path is null ? key : $"{path}.{key}") is { } found)
                {
                    return found;
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            for (var i = 0; i < jsonArray.Count; i++)
            {
                if (FindExpressionProperty(jsonArray[i], expression, $"{path}[{i}]") is { } found)
                {
                    return found;
                }
            }
        }

        return null;
    }

    // An expression that was cut matches the start of the property's.
    private static bool Matches(string candidate, string expression)
        => string.Equals(candidate, expression, StringComparison.Ordinal) ||
            (expression.EndsWith(Ellipsis, StringComparison.Ordinal) && candidate.StartsWith(expression[..^Ellipsis.Length], StringComparison.Ordinal));

    // Keeps the values within MaxDataLength, in this order: the last result, the outputs, the variables, the evaluations
    // and the other properties. The values left out are marked.
    private static void KeepWithinBudget(WorkflowExecutionData data)
    {
        var remaining = WorkflowExecutionData.MaxDataLength;

        bool Fits(int length)
        {
            if (length > remaining)
            {
                data.IsTruncated = true;

                return false;
            }

            remaining -= length;

            return true;
        }

        if (data.LastResult is not null && !Fits(data.LastResult.Length))
        {
            data.LastResult = null;
        }

        data.Outputs = Keep(data.Outputs, Fits);
        data.Variables = Keep(data.Variables, Fits);
        data.Evaluations = data.Evaluations.Where(evaluation => Fits((evaluation.Expression?.Length ?? 0) + (evaluation.Result?.Length ?? 0))).ToList();
        data.Properties = Keep(data.Properties, Fits);
    }

    private static Dictionary<string, string> Keep(IDictionary<string, string> values, Func<int, bool> fits)
        => values.Where(pair => fits(pair.Key.Length + (pair.Value?.Length ?? 0))).ToDictionary(pair => pair.Key, pair => pair.Value);
}

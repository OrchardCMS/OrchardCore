using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrchardCore.Workflows.Models;

/// <summary>
/// The data of an activity's execution, recorded in its journal record when its workflow records activity data
/// (<see cref="WorkflowType.RecordActivityData"/>): the expressions it evaluated, the outputs it set, the variables and
/// properties it changed, and its last result. Values are JSON text, cut when they are too long.
/// </summary>
public sealed class WorkflowExecutionData
{
    /// <summary>
    /// The length, in characters, beyond which a value is cut.
    /// </summary>
    public const int MaxValueLength = 2_000;

    /// <summary>
    /// The length, in characters, of the values a record keeps: the values beyond it are left out.
    /// </summary>
    public const int MaxDataLength = 32_000;

    private const string Ellipsis = "…";

    /// <summary>
    /// The expressions the activity evaluated, in order.
    /// </summary>
    public IList<WorkflowExpressionEvaluation> Evaluations { get; set; } = [];

    /// <summary>
    /// The outputs the activity set (see <see cref="Activities.IActivityOutputs"/>), by name.
    /// </summary>
    public IDictionary<string, string> Outputs { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// The declared variables the activity changed, by name, with their new value.
    /// </summary>
    public IDictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// The other workflow properties the activity changed, by name, with their new value.
    /// </summary>
    public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// The last result the activity set, or <see langword="null"/> when it didn't set one.
    /// </summary>
    public string LastResult { get; set; }

    /// <summary>
    /// Whether values were cut or left out to keep the record small.
    /// </summary>
    public bool IsTruncated { get; set; }

    /// <summary>
    /// Whether nothing was recorded.
    /// </summary>
    [JsonIgnore]
    public bool IsEmpty => Evaluations.Count == 0 && Outputs.Count == 0 && Variables.Count == 0 && Properties.Count == 0 && LastResult is null;

    /// <summary>
    /// Returns a value as JSON text, cut to <see cref="MaxValueLength"/> characters. A value that can't be serialized
    /// is shown as text.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="truncated">Whether the text was cut.</param>
    public static string FormatValue(object value, out bool truncated)
    {
        string text;

        try
        {
            text = value is null ? "null" : JConvert.SerializeObject(value, JOptions.Default);
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException or InvalidOperationException)
        {
            text = JConvert.SerializeObject(value.ToString(), JOptions.Default);
        }

        truncated = text.Length > MaxValueLength;

        return truncated ? text[..MaxValueLength] + Ellipsis : text;
    }
}

/// <summary>
/// An expression an activity evaluated, and its result.
/// </summary>
public sealed class WorkflowExpressionEvaluation
{
    /// <summary>
    /// The activity property the expression belongs to, such as <c>Condition</c> or <c>Inputs.amount</c>, or
    /// <see langword="null"/> when it isn't known.
    /// </summary>
    public string Property { get; set; }

    /// <summary>
    /// The syntax the expression was evaluated with, such as <c>JavaScript</c> or <c>Liquid</c>.
    /// </summary>
    public string Syntax { get; set; }

    /// <summary>
    /// The text of the expression, cut to <see cref="WorkflowExecutionData.MaxValueLength"/> characters.
    /// </summary>
    public string Expression { get; set; }

    /// <summary>
    /// The result, as JSON text (see <see cref="WorkflowExecutionData.FormatValue"/>).
    /// </summary>
    public string Result { get; set; }
}

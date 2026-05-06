using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using OrchardCore.DataOrchestrator.Models;

namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Base class for ETL activities.
/// </summary>
public abstract class EtlActivity : IEtlActivity
{
    public abstract string Name { get; }

    public abstract string DisplayText { get; }

    public abstract string Category { get; }

    public JsonObject Properties { get; set; } = [];

    public virtual bool HasEditor => true;

    public abstract IEnumerable<EtlOutcome> GetPossibleOutcomes();

    public abstract Task<EtlActivityResult> ExecuteAsync(EtlExecutionContext context);

    /// <summary>
    /// Returns an <see cref="EtlActivityResult"/> with the specified outcome names.
    /// </summary>
    protected static EtlActivityResult Outcomes(params string[] names)
    {
        return EtlActivityResult.Success(names);
    }

    /// <summary>
    /// Reads a property value from the <see cref="Properties"/> JSON object.
    /// </summary>
    protected virtual T GetProperty<T>(Func<T> defaultValue = null, [CallerMemberName] string name = null)
    {
        var item = Properties[name];

        return item != null
            ? item.ToObject<T>()
            : defaultValue != null
                ? defaultValue()
                : default;
    }

    /// <summary>
    /// Writes a property value to the <see cref="Properties"/> JSON object.
    /// </summary>
    protected virtual void SetProperty(object value, [CallerMemberName] string name = null)
    {
        Properties[name] = value is JsonNode node ? node.DeepClone() : JNode.FromObject(value);
    }
}

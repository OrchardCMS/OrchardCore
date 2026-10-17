using System.Security.Claims;

namespace OrchardCore.DataSources;

/// <summary>
/// Carries the caller information a data source needs to decide which data sets it may expose.
/// </summary>
public sealed class DataSourceContext
{
    private readonly List<Func<IServiceProvider, CancellationToken, Task>> _succeededActions = [];

    /// <summary>
    /// Gets or sets the principal the data is read for. A data source must hide every data set this principal is not
    /// allowed to read. While a definition is designed this is the designer; while a saved definition runs unattended
    /// it is the user who vouches for it, such as its owner or the user who published it.
    /// </summary>
    public ClaimsPrincipal User { get; set; }

    /// <summary>
    /// Gets or sets a stable identifier of what reads, such as a step of a data pipeline (<c>{pipelineId}/{stepId}</c>),
    /// so that a data source can keep a position for each consumer, such as the changes of a queue each consumer
    /// still has to read. <see langword="null"/> when the consumer has none, such as a preview.
    /// </summary>
    public string ConsumerId { get; set; }

    /// <summary>
    /// Gets the extensible bag of values a consumer and its data sources share during one run.
    /// </summary>
    public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the actions the data source asked to run once what it read was delivered.
    /// </summary>
    public IReadOnlyList<Func<IServiceProvider, CancellationToken, Task>> SucceededActions
    {
        get
        {
            lock (_succeededActions)
            {
                return [.. _succeededActions];
            }
        }
    }

    /// <summary>
    /// Adds an action to run once the consumer delivered what the data source read, such as to remove the changes it
    /// read from a queue. A consumer that confirms what it reads, such as a run of a data pipeline, runs it only when
    /// it succeeds, so what it didn't deliver is read again; other consumers, such as a preview, never run it.
    /// </summary>
    /// <param name="action">The action, which receives the services of a scope of the tenant.</param>
    public void OnSucceeded(Func<IServiceProvider, CancellationToken, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        lock (_succeededActions)
        {
            _succeededActions.Add(action);
        }
    }
}

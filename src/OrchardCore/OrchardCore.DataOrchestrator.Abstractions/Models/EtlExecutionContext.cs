using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using OrchardCore.DataOrchestrator.Services;

namespace OrchardCore.DataOrchestrator.Models;

/// <summary>
/// Provides context for ETL pipeline execution, analogous to WorkflowExecutionContext.
/// </summary>
public sealed class EtlExecutionContext
{
    public EtlExecutionContext(
        EtlPipelineDefinition pipeline,
        IEtlActivityLibrary activityLibrary,
        IServiceProvider serviceProvider,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        Pipeline = pipeline;
        ActivityLibrary = activityLibrary;
        ServiceProvider = serviceProvider;
        Logger = logger;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// Gets the pipeline definition being executed.
    /// </summary>
    public EtlPipelineDefinition Pipeline { get; }

    /// <summary>
    /// Gets the service provider for resolving dependencies.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the activity library for instantiating additional activities during execution.
    /// </summary>
    public IEtlActivityLibrary ActivityLibrary { get; }

    /// <summary>
    /// Gets the cancellation token for the execution.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets a dictionary for sharing state between activities.
    /// </summary>
    public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets a dictionary of parameter values provided when the pipeline was invoked.
    /// </summary>
    public IDictionary<string, object> Parameters { get; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets the logger for the execution.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets or sets the data stream flowing between activities.
    /// Source activities set this, transforms modify it, and loads consume it.
    /// </summary>
    public IAsyncEnumerable<JsonObject> DataStream { get; set; }

    public EtlExecutionContext Clone()
    {
        var clone = new EtlExecutionContext(Pipeline, ActivityLibrary, ServiceProvider, Logger, CancellationToken);

        foreach (var property in Properties)
        {
            clone.Properties[property.Key] = property.Value;
        }

        foreach (var parameter in Parameters)
        {
            clone.Parameters[parameter.Key] = parameter.Value;
        }

        return clone;
    }
}

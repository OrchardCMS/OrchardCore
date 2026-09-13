using System.Text.Json.Serialization;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Operations;

namespace OrchardCore.Indexing.Endpoints.Management;

/// <summary>Describes a tenant-local lifecycle operation without private provider diagnostics.</summary>
public sealed class IndexOperationResponse
{
    /// <summary>Gets the opaque operation identifier.</summary>
    public string Id { get; init; }
    /// <summary>Gets the target index profile identifier.</summary>
    public string IndexId { get; init; }
    /// <summary>Gets the requested lifecycle action.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter<IndexLifecycleAction>))]
    public IndexLifecycleAction Action { get; init; }
    /// <summary>Gets the observed operation state.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter<IndexOperationState>))]
    public IndexOperationState State { get; init; }
    /// <summary>Gets the request creation time.</summary>
    public DateTime CreatedUtc { get; init; }
    /// <summary>Gets the most recent state-transition time.</summary>
    public DateTime UpdatedUtc { get; init; }
    /// <summary>Gets the processing outcome, when known.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter<IndexProcessingStatus>))]
    public IndexProcessingStatus? Outcome { get; init; }
    /// <summary>Gets the last confirmed provider cursor.</summary>
    public long? LastTaskId { get; init; }
}

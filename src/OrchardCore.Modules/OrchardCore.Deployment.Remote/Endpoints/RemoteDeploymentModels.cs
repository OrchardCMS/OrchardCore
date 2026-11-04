namespace OrchardCore.Deployment.Remote.Endpoints;

/// <summary>Explicit remote client configuration; omitted API keys are preserved on update.</summary>
public sealed class RemoteClientRequest
{
    /// <summary>Gets or sets the target's case-sensitive client name.</summary>
    public string ClientName { get; set; }
    /// <summary>Gets or sets the write-only shared key; required for creation and never returned.</summary>
    public string ApiKey { get; set; }
}

/// <summary>Explicit outbound deployment destination configuration.</summary>
public sealed class RemoteInstanceRequest
{
    /// <summary>Gets or sets the unique display name.</summary>
    public string Name { get; set; }
    /// <summary>Gets or sets the remote import endpoint URL.</summary>
    public string Url { get; set; }
    /// <summary>Gets or sets the remote client's case-sensitive name.</summary>
    public string ClientName { get; set; }
    /// <summary>Gets or sets the write-only shared key; omitted keys are preserved on update.</summary>
    public string ApiKey { get; set; }
}

/// <summary>Credential-free client metadata.</summary>
public sealed class RemoteClientResponse
{
    /// <summary>Gets the tenant-local identity.</summary>
    public string Id { get; init; }
    /// <summary>Gets the client name.</summary>
    public string ClientName { get; init; }
    /// <summary>Gets whether a protected key is configured.</summary>
    public bool HasApiKey { get; init; }
}

/// <summary>Credential-free remote destination metadata.</summary>
public sealed class RemoteInstanceResponse
{
    /// <summary>Gets the tenant-local identity.</summary>
    public string Id { get; init; }
    /// <summary>Gets the display name.</summary>
    public string Name { get; init; }
    /// <summary>Gets the remote import URL.</summary>
    public string Url { get; init; }
    /// <summary>Gets the remote client name.</summary>
    public string ClientName { get; init; }
    /// <summary>Gets whether a key is configured.</summary>
    public bool HasApiKey { get; init; }
}

/// <summary>Reports whether a configuration mutation changed persisted data.</summary>
public sealed class RemoteDeploymentWriteResponse<T>
{
    /// <summary>Gets whether persisted data changed.</summary>
    public bool Changed { get; init; }
    /// <summary>Gets credential-free resource metadata, or null after deletion.</summary>
    public T Resource { get; init; }
}

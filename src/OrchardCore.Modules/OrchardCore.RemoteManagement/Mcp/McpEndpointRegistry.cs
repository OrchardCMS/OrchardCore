using Microsoft.AspNetCore.Routing;

namespace OrchardCore.RemoteManagement.Mcp;

// Each tenant owns this registry. Keep the live collection so all module routes are included
// after startup finishes, without consulting the host's endpoint data sources.
internal sealed class McpEndpointRegistry
{
    public ICollection<EndpointDataSource> DataSources { get; set; } = [];

    // Built on first use, once all module routes exist. Tools don't depend on the caller, and a
    // feature change releases the tenant and this registry with it.
    public IReadOnlyList<McpToolDefinition> Tools { get; set; }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Queries.Endpoints.Api;

internal static class QueryManagementEndpointConventions
{
    public const string CapabilityName = "queries";
    public const string TagName = "Queries";
}

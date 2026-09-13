using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Deployment.Remote.Models;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment.Remote.Endpoints;

internal static class RemoteDeploymentEndpoints
{
    internal static void Map(IEndpointRouteBuilder routes)
    {
        foreach (var clients in new[] { true, false })
        {
            var resource = clients ? "remote-clients" : "remote-instances";
            var permission = clients ? DeploymentPermissions.ManageRemoteClients : DeploymentPermissions.ManageRemoteInstances;
            var prefix = clients ? "RemoteDeploymentClients" : "RemoteDeploymentInstances";
            var path = "api/deployment/" + resource;
            if (clients)
            {
                Configure(routes.MapGet(path, ListClientsAsync), prefix + "List", resource, "list", permission).Produces<RemoteClientResponse[]>();
                Configure(routes.MapGet(path + "/{id}", GetClientAsync), prefix + "Get", resource, "show", permission, true).Produces<RemoteClientResponse>();
                Configure(routes.MapPost(path, CreateClientAsync), prefix + "Create", resource, "create", permission).Produces<RemoteDeploymentWriteResponse<RemoteClientResponse>>();
                Configure(routes.MapPut(path + "/{id}", UpdateClientAsync), prefix + "Update", resource, "update", permission, true).Produces<RemoteDeploymentWriteResponse<RemoteClientResponse>>();
                Configure(routes.MapDelete(path + "/{id}", DeleteClientAsync), prefix + "Delete", resource, "delete", permission, true).Produces<RemoteDeploymentWriteResponse<RemoteClientResponse>>();
            }
            else
            {
                Configure(routes.MapGet(path, ListInstancesAsync), prefix + "List", resource, "list", permission).Produces<RemoteInstanceResponse[]>();
                Configure(routes.MapGet(path + "/{id}", GetInstanceAsync), prefix + "Get", resource, "show", permission, true).Produces<RemoteInstanceResponse>();
                Configure(routes.MapPost(path, CreateInstanceAsync), prefix + "Create", resource, "create", permission).Produces<RemoteDeploymentWriteResponse<RemoteInstanceResponse>>();
                Configure(routes.MapPut(path + "/{id}", UpdateInstanceAsync), prefix + "Update", resource, "update", permission, true).Produces<RemoteDeploymentWriteResponse<RemoteInstanceResponse>>();
                Configure(routes.MapDelete(path + "/{id}", DeleteInstanceAsync), prefix + "Delete", resource, "delete", permission, true).Produces<RemoteDeploymentWriteResponse<RemoteInstanceResponse>>();
            }
        }
    }

    internal static RouteHandlerBuilder Configure(RouteHandlerBuilder route, string name, string resource, string verb, Permission permission, bool id = false)
    {
        var metadata = new CliOperationMetadata(["deployment", resource], verb)
        {
            Capability = "deployment-remote", RequiresConfirmation = verb is "delete" or "send",
        };
        if (verb is "create" or "update") { metadata.SecretProperties.Add("apiKey"); }
        if (id) { metadata.Arguments.Add(new CliArgumentMetadata("id", 0)); }
        return route.WithName(name).WithTags("Remote Deployment").WithSummary(verb + " " + resource + "; credentials are never returned.")
            .WithCliCommand(metadata).RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
                .RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement), new PermissionRequirement(permission)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403).ProducesProblem(404).ProducesProblem(409);
    }

    private static ProblemHttpResult Secure(HttpContext context) => context.Request.IsHttps ? null : TypedResults.Problem("HTTPS is required.", statusCode: 400);
    private static ProblemHttpResult Conflict() => TypedResults.Problem("A resource with this name already exists with different configuration.", statusCode: 409);
    private static RemoteClientResponse Describe(RemoteClient value) => new() { Id = value.Id, ClientName = value.ClientName, HasApiKey = value.ProtectedApiKey?.Length > 0 };
    private static RemoteInstanceResponse Describe(RemoteInstance value) => new() { Id = value.Id, Name = value.Name, Url = value.Url, ClientName = value.ClientName, HasApiKey = !string.IsNullOrEmpty(value.ApiKey) };

    private static async Task<IResult> ListClientsAsync([FromServices] RemoteClientService service) => TypedResults.Ok((await service.GetRemoteClientListAsync()).RemoteClients.OrderBy(value => value.ClientName, StringComparer.Ordinal).Select(Describe).ToArray());
    private static async Task<IResult> GetClientAsync(string id, [FromServices] RemoteClientService service) => await service.GetRemoteClientAsync(id) is { } value ? TypedResults.Ok(Describe(value)) : TypedResults.NotFound();
    private static async Task<IResult> CreateClientAsync(HttpContext context, [FromServices] RemoteClientService service, [FromBody] RemoteClientRequest request)
    {
        if (Secure(context) is { } failure) { return failure; }
        if (request is null) { return TypedResults.BadRequest(); }
        var errors = RemoteDeploymentValidation.Client(request.ClientName, request.ApiKey);
        if (errors.Count > 0) { return TypedResults.ValidationProblem(errors); }
        var existing = (await service.GetRemoteClientListAsync()).RemoteClients.FirstOrDefault(value => value.ClientName == request.ClientName);
        if (existing is not null)
        {
            return service.MatchesApiKey(existing, request.ApiKey) ? TypedResults.Ok(new RemoteDeploymentWriteResponse<RemoteClientResponse> { Resource = Describe(existing) }) : Conflict();
        }
        var created = await service.CreateRemoteClientAsync(request.ClientName, request.ApiKey);
        return TypedResults.Ok(new RemoteDeploymentWriteResponse<RemoteClientResponse> { Changed = true, Resource = Describe(created) });
    }
    private static async Task<IResult> UpdateClientAsync(string id, HttpContext context, [FromServices] RemoteClientService service, [FromBody] RemoteClientRequest request)
    {
        if (Secure(context) is { } failure) { return failure; }
        if (request is null) { return TypedResults.BadRequest(); }
        var value = await service.GetRemoteClientAsync(id);
        if (value is null) { return TypedResults.NotFound(); }
        var errors = RemoteDeploymentValidation.Client(request.ClientName, request.ApiKey ?? "preserved");
        if (errors.Count > 0) { return TypedResults.ValidationProblem(errors); }
        if ((await service.GetRemoteClientListAsync()).RemoteClients.Any(other => other.Id != value.Id && other.ClientName == request.ClientName)) { return Conflict(); }
        var changed = value.ClientName != request.ClientName || request.ApiKey is not null && !service.MatchesApiKey(value, request.ApiKey);
        if (changed)
        {
            if (request.ApiKey is null) { await service.RenameAsync(value, request.ClientName); }
            else { await service.TryUpdateRemoteClient(id, request.ClientName, request.ApiKey); }
        }
        return TypedResults.Ok(new RemoteDeploymentWriteResponse<RemoteClientResponse> { Changed = changed, Resource = Describe(value) });
    }
    private static async Task<IResult> DeleteClientAsync(string id, HttpContext context, [FromServices] RemoteClientService service)
    {
        if (Secure(context) is { } failure) { return failure; }
        var exists = await service.GetRemoteClientAsync(id) is not null;
        if (exists) { await service.DeleteRemoteClientAsync(id); }
        return TypedResults.Ok(new RemoteDeploymentWriteResponse<RemoteClientResponse> { Changed = exists });
    }

    private static async Task<IResult> ListInstancesAsync([FromServices] RemoteInstanceService service) => TypedResults.Ok((await service.GetRemoteInstanceListAsync()).RemoteInstances.OrderBy(value => value.Name, StringComparer.Ordinal).Select(Describe).ToArray());
    private static async Task<IResult> GetInstanceAsync(string id, [FromServices] RemoteInstanceService service) => await service.GetRemoteInstanceAsync(id) is { } value ? TypedResults.Ok(Describe(value)) : TypedResults.NotFound();
    private static async Task<IResult> CreateInstanceAsync(HttpContext context, [FromServices] RemoteInstanceService service, [FromBody] RemoteInstanceRequest request)
    {
        if (Secure(context) is { } failure) { return failure; }
        if (request is null) { return TypedResults.BadRequest(); }
        var errors = RemoteDeploymentValidation.Instance(request.Name, request.Url, request.ClientName, request.ApiKey);
        if (errors.Count > 0) { return TypedResults.ValidationProblem(errors); }
        var existing = (await service.GetRemoteInstanceListAsync()).RemoteInstances.FirstOrDefault(value => value.Name == request.Name);
        if (existing is not null)
        {
            return existing.Url == request.Url && existing.ClientName == request.ClientName && existing.ApiKey == request.ApiKey
                ? TypedResults.Ok(new RemoteDeploymentWriteResponse<RemoteInstanceResponse> { Resource = Describe(existing) }) : Conflict();
        }
        await service.CreateRemoteInstanceAsync(request.Name, request.Url, request.ClientName, request.ApiKey);
        var created = (await service.LoadRemoteInstanceListAsync()).RemoteInstances.Last(value => value.Name == request.Name);
        return TypedResults.Ok(new RemoteDeploymentWriteResponse<RemoteInstanceResponse> { Changed = true, Resource = Describe(created) });
    }
    private static async Task<IResult> UpdateInstanceAsync(string id, HttpContext context, [FromServices] RemoteInstanceService service, [FromBody] RemoteInstanceRequest request)
    {
        if (Secure(context) is { } failure) { return failure; }
        if (request is null) { return TypedResults.BadRequest(); }
        var value = await service.GetRemoteInstanceAsync(id);
        if (value is null) { return TypedResults.NotFound(); }
        var key = request.ApiKey ?? value.ApiKey;
        var errors = RemoteDeploymentValidation.Instance(request.Name, request.Url, request.ClientName, key);
        if (errors.Count > 0) { return TypedResults.ValidationProblem(errors); }
        if ((await service.GetRemoteInstanceListAsync()).RemoteInstances.Any(other => other.Id != value.Id && other.Name == request.Name)) { return Conflict(); }
        var changed = value.Name != request.Name || value.Url != request.Url || value.ClientName != request.ClientName || value.ApiKey != key;
        if (changed) { await service.UpdateRemoteInstance(id, request.Name, request.Url, request.ClientName, key); }
        return TypedResults.Ok(new RemoteDeploymentWriteResponse<RemoteInstanceResponse> { Changed = changed, Resource = Describe(await service.LoadRemoteInstanceAsync(id)) });
    }
    private static async Task<IResult> DeleteInstanceAsync(string id, HttpContext context, [FromServices] RemoteInstanceService service)
    {
        if (Secure(context) is { } failure) { return failure; }
        var exists = await service.GetRemoteInstanceAsync(id) is not null;
        if (exists) { await service.DeleteRemoteInstanceAsync(id); }
        return TypedResults.Ok(new RemoteDeploymentWriteResponse<RemoteInstanceResponse> { Changed = exists });
    }
}

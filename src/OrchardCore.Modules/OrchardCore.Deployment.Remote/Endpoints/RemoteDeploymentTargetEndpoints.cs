using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Deployment.Services;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Recipes.Models;
using YesSql;
using ISession = YesSql.ISession;

namespace OrchardCore.Deployment.Remote.Endpoints;

internal static class RemoteDeploymentTargetEndpoints
{
    internal static void Map(IEndpointRouteBuilder routes)
    {
        RemoteDeploymentEndpoints.Configure(routes.MapGet("api/deployment/targets", ListAsync), "RemoteDeploymentTargetsList", "targets", "list", DeploymentPermissions.ExportRemoteInstances)
            .Produces<RemoteDeploymentTargetResponse[]>();
        RemoteDeploymentEndpoints.Configure(routes.MapPost("api/deployment/targets/{id}/send", SendAsync), "RemoteDeploymentTargetsSend", "targets", "send", DeploymentPermissions.ExportRemoteInstances, true)
            .Produces<RemoteDeploymentSendResponse>().ProducesProblem(502);
    }

    private static async Task<IResult> ListAsync([FromServices] RemoteInstanceService service) => TypedResults.Ok(
        (await service.GetRemoteInstanceListAsync()).RemoteInstances.OrderBy(value => value.Name, StringComparer.Ordinal)
            .Select(value => new RemoteDeploymentTargetResponse { Id = value.Id, Name = value.Name }).ToArray());

    private static async Task<IResult> SendAsync(string id, HttpContext context, [FromServices] RemoteInstanceService instances,
        [FromServices] IAuthorizationService authorization, [FromServices] ISession session, [FromServices] IDeploymentArchiveService archives,
        [FromServices] RemoteDeploymentSender sender, [FromBody] RemoteDeploymentSendRequest request)
    {
        if (!context.Request.IsHttps) { return TypedResults.Problem("HTTPS is required.", statusCode: 400); }
        if (!await authorization.AuthorizeAsync(context.User, DeploymentPermissions.Export)) { return context.ApiForbidProblem(); }
        if (request is null || request.PlanId <= 0) { return TypedResults.Problem("A positive plan ID is required.", statusCode: 400); }
        var destination = await instances.GetRemoteInstanceAsync(id);
        var plan = await session.GetAsync<DeploymentPlan>(request.PlanId);
        if (destination is null || plan is null) { return TypedResults.NotFound(); }
        if (!RemoteDeploymentValidation.IsSafeUrl(destination.Url)) { return TypedResults.Problem("Update the destination to a supported HTTPS import URL.", statusCode: 400); }
        try
        {
            await using var archive = await archives.CreateAsync(plan, new RecipeDescriptor());
            var status = await sender.SendAsync(destination, archive, plan.Name.ToSafeName() + ".zip", context.RequestAborted);
            return status == HttpStatusCode.OK ? TypedResults.Ok(new RemoteDeploymentSendResponse { Succeeded = true })
                : TypedResults.Problem("The remote import did not report success. Inspect the target before sending again.", statusCode: 502);
        }
        catch (UnauthorizedAccessException) { return context.ApiForbidProblem(); }
        catch (Exception exception) when (exception is HttpRequestException or OperationCanceledException)
        {
            return TypedResults.Problem("Remote deployment completion is unknown. Inspect the target before sending again.", statusCode: 502);
        }
    }
}

/// <summary>Identifies an existing remote instance without exposing its connection credentials.</summary>
public sealed class RemoteDeploymentTargetResponse
{
    /// <summary>Gets the tenant-local remote instance identity.</summary>
    public string Id { get; init; }
    /// <summary>Gets the destination display name.</summary>
    public string Name { get; init; }
}

/// <summary>Requests one remote send of a tenant-local deployment plan.</summary>
public sealed class RemoteDeploymentSendRequest
{
    /// <summary>Gets or sets the plan to export and send.</summary>
    public long PlanId { get; set; }
}

/// <summary>Reports the remote endpoint's successful import response.</summary>
public sealed class RemoteDeploymentSendResponse
{
    /// <summary>Gets whether the target reported success.</summary>
    public bool Succeeded { get; init; }
}

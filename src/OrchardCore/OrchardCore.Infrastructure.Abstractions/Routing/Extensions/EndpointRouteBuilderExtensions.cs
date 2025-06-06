using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Routing.Extensions;

public static class EndpointRouteBuilderExtensions
{
    private const string ApiPrefix = "api";

    private static readonly IAuthorizeData _authorizeData = new AuthorizeAttribute
    {
        AuthenticationSchemes = "Api",
    };

    public static IEndpointRouteBuilder MapGet(this IEndpointRouteBuilder builder, Delegate handler, string pattern = "")
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(handler);

        builder.MapGet(pattern, handler)
            .WithName(handler.Method.Name);

        return builder;
    }

    public static IEndpointRouteBuilder MapPost(this IEndpointRouteBuilder builder, Delegate handler, string pattern = "")
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(handler);

        builder.MapPost(pattern, handler)
            .WithName(handler.Method.Name);

        return builder;
    }

    public static IEndpointRouteBuilder MapPut(this IEndpointRouteBuilder builder, Delegate handler, string pattern)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(handler);

        builder.MapPut(pattern, handler)
            .WithName(handler.Method.Name);

        return builder;
    }

    public static IEndpointRouteBuilder MapDelete(this IEndpointRouteBuilder builder, Delegate handler, string pattern)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(handler);

        builder.MapDelete(pattern, handler)
            .WithName(handler.Method.Name);

        return builder;
    }

    public static RouteGroupBuilder MapGroup(this IEndpointRouteBuilder builder, IEndpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(endpoint);

        var groupName = endpoint.GetGroupName();

        return builder
            .MapGroup($"{ApiPrefix}/{groupName}")
            .WithGroupName(groupName)
            .WithTags(groupName)
            .RequireAuthorization(_authorizeData)
            .AllowAnonymous()
            .DisableAntiforgery();
    }

    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder builder)
    {
        var stackTrace = new StackTrace();
        var callerMethodAssembly = stackTrace.GetFrame(1).GetMethod().DeclaringType.Assembly;

        var endpointTypes = callerMethodAssembly
            .GetExportedTypes()
            .Where(t => t.IsAssignableTo(typeof(IEndpoint)));

        foreach (var type in endpointTypes)
        {
            if (Activator.CreateInstance(type) is IEndpoint endpointGroup)
            {
                endpointGroup.Map(builder);
            }
        }

        return builder;
    }
}

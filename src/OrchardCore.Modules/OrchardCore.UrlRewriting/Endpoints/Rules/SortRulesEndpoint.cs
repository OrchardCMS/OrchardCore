using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.UrlRewriting.Endpoints.Rules;

public static class SortRulesEndpoint
{
    public const string RouteName = "ResortUrlRewritingRules";

    public static IEndpointRouteBuilder AddSortRulesEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("url-rewriting/resort", HandleAsync)
            .AllowAnonymous()
            .WithName(RouteName)
            .DisableAntiforgery();

        return builder;
    }

    private static async Task<IResult> HandleAsync(
        IAuthorizationService authorizationService,
        HttpContext httpContext,
        IRewriteRulesManager rewriteRulesManager,
        ResortingRequest model)
    {

        if (!model.OldIndex.HasValue || !model.NewIndex.HasValue)
        {
            return TypedResults.BadRequest();
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, UrlRewritingPermissions.ManageUrlRewritingRules))
        {
            return TypedResults.Forbid();
        }

        await rewriteRulesManager.ResortOrderAsync(model.OldIndex.Value, model.NewIndex.Value);

        return TypedResults.Ok();
    }

    private sealed class ResortingRequest
    {
        public int? OldIndex { get; set; }

        public int? NewIndex { get; set; }
    }
}

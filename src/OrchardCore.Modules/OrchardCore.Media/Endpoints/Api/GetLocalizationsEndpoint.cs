using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Localization;

namespace OrchardCore.Media.Endpoints.Api;

public static class GetLocalizationsEndpoint
{
    public static IEndpointRouteBuilder AddGetLocalizationsEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/media/localizations", HandleAsync)
            .WithName("ApiGetMediaLocalizations")
            .WithTags("MediaApi")
            .AllowAnonymous()
            .DisableAntiforgery()
            .Produces<Dictionary<string, string>>(StatusCodes.Status200OK);

        return builder;
    }

    // Anonymous by design: the standalone gallery loads its UI labels before the user authenticates,
    // and these are non-sensitive display strings — the same set the embedded admin page renders via
    // Orchard.GetJSLocalizations("media-gallery"). Aggregates every IJSLocalizer for the group and
    // returns the strings for the request's resolved culture.
    private static Ok<Dictionary<string, string>> HandleAsync(IEnumerable<IJSLocalizer> jsLocalizers)
    {
        var result = new Dictionary<string, string>();

        foreach (var jsLocalizer in jsLocalizers)
        {
            var localizations = jsLocalizer.GetLocalizations("media-gallery");
            if (localizations is null)
            {
                continue;
            }

            foreach (var kvp in localizations)
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        return TypedResults.Ok(result);
    }
}

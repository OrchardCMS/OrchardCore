using Microsoft.AspNetCore.Http;

namespace OrchardCore.Security;

/// <summary>
/// Attaches an RFC 9457 Problem Details body to the 401/403 responses produced by the "Api"
/// authentication scheme. For a resource server, RFC 6750 conveys the error details in the
/// WWW-Authenticate header, which is the responsibility of the authentication handler that
/// issued the challenge (e.g. the OpenIddict validation handler): this middleware never touches
/// the status code or the headers and only takes care of the body, so JavaScript API clients get
/// a parseable payload without having to parse the header.
/// </summary>
/// <remarks>
/// The body is written through <see cref="IProblemDetailsService"/> so app-level Problem Details
/// customizations are honored. The status code pages middleware isn't used for that purpose, as
/// the API responses deliberately disable it: the Diagnostics feature registers it around the
/// tenant pipeline to substitute HTML error pages, which API clients must never receive.
/// </remarks>
public sealed class ApiProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IProblemDetailsService _problemDetailsService;

    public ApiProblemDetailsMiddleware(RequestDelegate next, IProblemDetailsService problemDetailsService)
    {
        _next = next;
        _problemDetailsService = problemDetailsService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // Only the responses issued by the "Api" scheme are eligible: they are the ones left
        // body-less on purpose by the authentication handlers that produced them.
        if (context.Features.Get<ApiChallengeFeature>() is null)
        {
            return;
        }

        var response = context.Response;
        if (response.HasStarted ||
            response.StatusCode < 400 ||
            response.ContentLength.HasValue ||
            !string.IsNullOrEmpty(response.ContentType))
        {
            return;
        }

        await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = { Status = response.StatusCode },
        });
    }
}

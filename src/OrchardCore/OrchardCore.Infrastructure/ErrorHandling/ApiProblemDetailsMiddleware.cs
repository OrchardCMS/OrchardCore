using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Infrastructure.ErrorHandling;

/// <summary>
/// Attaches an RFC 9457 Problem Details body to the error responses that opted out of the HTML
/// error pages, i.e. the responses meant for headless clients: the ones produced by the "Api"
/// authentication scheme, which disables the status code pages, and the ones produced by an
/// endpoint marked with <see cref="ISkipStatusCodePagesMetadata"/>. Responses that didn't opt out
/// are left untouched for the status code pages middleware registered by the Diagnostics feature.
/// </summary>
/// <remarks>
/// Status codes and headers are never altered: for a resource server, RFC 6750 conveys the error
/// details in the WWW-Authenticate header, which remains the responsibility of the authentication
/// handler that issued the challenge (e.g. the OpenIddict validation handler). This middleware
/// only takes care of the body, so JavaScript API clients get a parseable payload without having
/// to parse the header. It is written through <see cref="IProblemDetailsService"/> so app-level
/// Problem Details customizations are honored.
/// </remarks>
public sealed class ApiProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger _logger;

    public ApiProblemDetailsMiddleware(
        RequestDelegate next,
        IProblemDetailsService problemDetailsService,
        ILogger<ApiProblemDetailsMiddleware> logger)
    {
        _next = next;
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        var response = context.Response;
        if (response.HasStarted ||
            response.StatusCode < 400 ||
            response.ContentLength.HasValue ||
            !string.IsNullOrEmpty(response.ContentType))
        {
            return;
        }

        // Note: the status code pages feature is resolved after the rest of the pipeline has run,
        // when the components that opt out of the HTML error pages - like the "Api" authentication
        // handler - have had a chance to disable it for the current response.
        if (context.Features.Get<IStatusCodePagesFeature>() is not { Enabled: false } &&
            context.GetEndpoint()?.Metadata.GetMetadata<ISkipStatusCodePagesMetadata>() is null)
        {
            return;
        }

        var problemDetails = new ProblemDetails
        {
            Status = response.StatusCode,
        };

        // Describe the two status codes this middleware is primarily concerned with. The machine-readable
        // details of a resource server challenge are conveyed by the WWW-Authenticate header (RFC 6750),
        // that the description points to. The "title" and "type" nodes of the other status codes are
        // resolved from the RFC 9110 defaults by the Problem Details service.
        var S = context.RequestServices.GetRequiredService<IStringLocalizer<ApiProblemDetailsMiddleware>>();

        switch (response.StatusCode)
        {
            case StatusCodes.Status401Unauthorized:
                problemDetails.Title = S["Authentication required"];
                problemDetails.Detail = S["Authentication is required to access this resource. Additional details may be found in the WWW-Authenticate HTTP response header."];
                break;

            case StatusCodes.Status403Forbidden:
                problemDetails.Title = S["Access forbidden"];
                problemDetails.Detail = S["Access to this resource is forbidden or you do not have sufficient permissions to perform this action."];
                break;
        }

        if (!await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problemDetails,
        }) && _logger.IsEnabled(LogLevel.Debug))
        {
            // The only expected reason is content negotiation - the client explicitly asked for a
            // media type no registered writer can produce - in which case the response is legitimately
            // returned without a body, exactly like the default status code pages handler does.
            _logger.LogDebug(
                "No Problem Details body was written for the {StatusCode} response returned for '{Path}'.",
                response.StatusCode, context.Request.Path);
        }
    }
}

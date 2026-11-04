using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Mvc;

/// <summary>
/// The stable, machine-readable failure kinds reported in the <c>code</c> extension of the Problem Details
/// responses of the APIs. Clients should match these values rather than the localized title and detail.
/// </summary>
public static class ApiProblemCodes
{
    /// <summary>The name of the Problem Details extension holding the code.</summary>
    public const string ExtensionName = "code";

    /// <summary>The request is invalid (400).</summary>
    public const string Validation = "validation";

    /// <summary>The request has no valid credentials (401).</summary>
    public const string Unauthorized = "unauthorized";

    /// <summary>The caller isn't allowed to perform the request (403).</summary>
    public const string Forbidden = "forbidden";

    /// <summary>The resource doesn't exist (404).</summary>
    public const string NotFound = "not_found";

    /// <summary>The request conflicts with the current state of the resource (409).</summary>
    public const string Conflict = "conflict";

    /// <summary>The <c>If-Match</c> header doesn't match the current version of the resource (412).</summary>
    public const string PreconditionFailed = "precondition_failed";

    /// <summary>Too many requests were sent, and the <c>Retry-After</c> header may tell when to retry (429).</summary>
    public const string RateLimited = "rate_limited";

    /// <summary>
    /// Gets the code of a failure from its status code.
    /// </summary>
    /// <param name="statusCode">The status code of the response.</param>
    /// <returns>The code, or <see langword="null"/> when the status code isn't one of the shared failure kinds.</returns>
    public static string FromStatusCode(int? statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => Validation,
        StatusCodes.Status401Unauthorized => Unauthorized,
        StatusCodes.Status403Forbidden => Forbidden,
        StatusCodes.Status404NotFound => NotFound,
        StatusCodes.Status409Conflict => Conflict,
        StatusCodes.Status412PreconditionFailed => PreconditionFailed,
        StatusCodes.Status429TooManyRequests => RateLimited,
        _ => null,
    };

    /// <summary>
    /// Adds the code of a failure to its Problem Details, keeping the one an endpoint chose, if any.
    /// </summary>
    /// <param name="context">The Problem Details being written.</param>
    public static void Apply(ProblemDetailsContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problemDetails = context.ProblemDetails;
        if (FromStatusCode(problemDetails.Status ?? context.HttpContext.Response.StatusCode) is { } code)
        {
            problemDetails.Extensions.TryAdd(ExtensionName, code);
        }
    }
}

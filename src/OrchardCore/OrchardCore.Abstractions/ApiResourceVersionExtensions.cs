using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Mvc;

/// <summary>
/// Optimistic concurrency for the APIs replacing a resource: reads and writes return the version of the resource
/// in the <c>ETag</c> header, and a write sending it back in the optional <c>If-Match</c> header is refused with
/// a 412 when the resource changed in the meantime, instead of silently overwriting that change.
/// </summary>
/// <remarks>
/// The version is a hash of the state passed by the endpoint, the same for its reads and writes. Leave out what the
/// write doesn't replace and what changes on its own, such as a login counter or a URL resolved from the request, as
/// a change of these would refuse a write that loses nothing. The check detects stale writes, and doesn't serialize
/// two requests running at the very same time.
/// </remarks>
public static class ApiResourceVersionExtensions
{
    /// <summary>
    /// Computes the strong entity tag of a resource from its state, regardless of the order of the JSON properties.
    /// </summary>
    /// <param name="state">The state of the resource, which should exclude secrets.</param>
    /// <returns>The quoted entity tag.</returns>
    public static string ComputeApiETag(object state)
    {
        ArgumentNullException.ThrowIfNull(state);

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            WriteCanonical(writer, JsonSerializer.SerializeToNode(state, state.GetType(), JOptions.Default));
        }

        return $"\"{Convert.ToHexStringLower(SHA256.HashData(stream.ToArray()).AsSpan(0, 16))}\"";
    }

    /// <summary>
    /// Sets the <c>ETag</c> header of the response to the version of the resource.
    /// </summary>
    /// <param name="httpContext">The current request.</param>
    /// <param name="state">The state of the resource.</param>
    public static void SetApiETag(this HttpContext httpContext, object state)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.Headers.ETag = ComputeApiETag(state);
    }

    /// <summary>
    /// Sets the <c>ETag</c> header of the response to the version of a resource whose state is what the response returns.
    /// </summary>
    /// <typeparam name="T">The type of the response.</typeparam>
    /// <param name="httpContext">The current request.</param>
    /// <param name="state">The state of the resource.</param>
    /// <returns>The state of the resource.</returns>
    public static T WithApiETag<T>(this HttpContext httpContext, T state)
    {
        httpContext.SetApiETag(state);

        return state;
    }

    /// <summary>
    /// Evaluates the optional <c>If-Match</c> header of the request against the current version of the resource.
    /// </summary>
    /// <param name="httpContext">The current request.</param>
    /// <param name="currentState">The state of the resource before the write.</param>
    /// <returns>
    /// A Problem with a 412 Precondition Failed status code and the current version in the <c>ETag</c> header when
    /// the header matches no version, or <see langword="null"/> when the header matches or is absent.
    /// </returns>
    public static IResult ApiIfMatchProblem(this HttpContext httpContext, object currentState)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (httpContext.Request.Headers.IfMatch.Count == 0)
        {
            return null;
        }

        var current = new EntityTagHeaderValue(ComputeApiETag(currentState));
        if (httpContext.Request.GetTypedHeaders().IfMatch.Any(tag => tag.Equals(EntityTagHeaderValue.Any) || tag.Compare(current, useStrongComparison: true)))
        {
            return null;
        }

        httpContext.Response.Headers.ETag = current.ToString();

        var S = httpContext.RequestServices.GetRequiredService<IStringLocalizer<ProblemDetailsApiLocalization>>();

        return TypedResults.Problem(
            title: S["Precondition failed"],
            detail: S["The resource changed since it was read. Read it again, reapply your change, and send its current ETag in the If-Match header."],
            statusCode: StatusCodes.Status412PreconditionFailed);
    }

    private static void WriteCanonical(Utf8JsonWriter writer, JsonNode node)
    {
        switch (node)
        {
            case JsonObject jsonObject:
                writer.WriteStartObject();
                foreach (var property in jsonObject.Where(property => property.Value is not null).OrderBy(property => property.Key, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Key);
                    WriteCanonical(writer, property.Value);
                }

                writer.WriteEndObject();
                break;

            case JsonArray jsonArray:
                writer.WriteStartArray();
                foreach (var item in jsonArray)
                {
                    WriteCanonical(writer, item);
                }

                writer.WriteEndArray();
                break;

            case null:
                writer.WriteNullValue();
                break;

            default:
                node.WriteTo(writer);
                break;
        }
    }
}

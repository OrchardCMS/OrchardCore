using System.Net;
using System.Net.Mime;
using GraphQL;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Apis.GraphQL;

internal static class DocumentWriterExtensions
{
    public static async Task WriteErrorAsync(this IGraphQLSerializer graphQLSerializer, HttpContext context, string message, Exception e = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        var errorResult = new ExecutionResult
        {
            Errors = []
        };

        if (e == null)
        {
            errorResult.Errors.Add(new ExecutionError(message));
        }
        else
        {
            errorResult.Errors.Add(new ExecutionError(message, e));
        }

        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        await graphQLSerializer.WriteAsync(context.Response.Body, errorResult);
    }
}

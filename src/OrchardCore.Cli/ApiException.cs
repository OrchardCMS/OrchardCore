using System.Text.Json.Nodes;

namespace OrchardCore.Cli;

internal sealed class ApiException : Exception
{
    public ApiException(
        int statusCode,
        string message,
        JsonNode? details,
        string? correlationId)
        : base(message)
    {
        StatusCode = statusCode;
        Details = details;
        CorrelationId = correlationId;
    }

    public int StatusCode { get; }

    public JsonNode? Details { get; }

    public string? CorrelationId { get; }
}

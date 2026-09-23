namespace OrchardCore.Workflows.Http.Services;

internal sealed class HttpRequestDestinationNotAllowedException : Exception
{
    public HttpRequestDestinationNotAllowedException(string message)
        : base(message)
    {
    }
}

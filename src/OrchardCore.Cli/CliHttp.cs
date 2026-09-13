using System.Net;

namespace OrchardCore.Cli;

internal static class CliHttp
{
    public static HttpClient CreateClient()
    {
        var handler = new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            AllowAutoRedirect = false,
        };

        return new HttpClient(handler)
        {
            MaxResponseContentBufferSize = 64 * 1024 * 1024,
            Timeout = TimeSpan.FromSeconds(30),
        };
    }
}

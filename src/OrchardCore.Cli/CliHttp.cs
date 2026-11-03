using System.Net;

namespace OrchardCore.Cli;

internal static class CliHttp
{
    /// <summary>The time a request has to complete, unless it uploads or downloads a stream.</summary>
    public static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(30);

    /// <summary>The time an operation that does its work before responding has to complete, such as a tenant setup.</summary>
    public static readonly TimeSpan LongRunningRequestTimeout = TimeSpan.FromMinutes(10);

    /// <summary>Gets the longest time limit that can be requested, in seconds: one day.</summary>
    public const int MaximumRequestTimeoutSeconds = 86_400;

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
            // Stream transfers take as long as they need, so each other request sets its own limit.
            Timeout = Timeout.InfiniteTimeSpan,
        };
    }

    /// <summary>Sends a request and reads its whole response within a time limit.</summary>
    public static async Task<HttpResponseMessage> SendWithTimeoutAsync(this HttpClient client, HttpRequestMessage request, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        source.CancelAfter(timeout);
        try
        {
            return await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, source.Token);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw CreateTimeoutException(exception);
        }
    }

    /// <summary>Creates the exception of a request that ran out of time, which <see cref="HttpClient"/> reports the same way.</summary>
    public static TaskCanceledException CreateTimeoutException(OperationCanceledException exception) =>
        new(exception.Message, new TimeoutException(exception.Message, exception));

    /// <summary>Tells a request that ran out of time from one that the user cancelled.</summary>
    public static bool IsTimeout(TaskCanceledException exception) => exception.InnerException is TimeoutException;
}

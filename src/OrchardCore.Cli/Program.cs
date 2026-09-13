using System.Text.Json;

namespace OrchardCore.Cli;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            using var httpClient = CliHttp.CreateClient();
            var app = await CliApplication.CreateAsync(args, CliPaths.CreateDefault(), httpClient, CancellationToken.None);
            return await app.InvokeAsync(args);
        }
        catch (Exception exception) when (exception is CliException or IOException or UnauthorizedAccessException or JsonException or HttpRequestException or OperationCanceledException)
        {
            // Discovery/configuration can fail before the command tree exists.
            // Never expose an unhandled stack trace (which can include request data).
            await Console.Error.WriteLineAsync($"Error: {exception.Message}");
            return exception switch
            {
                HttpRequestException => 2,
                JsonException => 3,
                OperationCanceledException => 130,
                _ => 1,
            };
        }
    }
}

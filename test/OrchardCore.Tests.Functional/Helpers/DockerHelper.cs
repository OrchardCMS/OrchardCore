namespace OrchardCore.Tests.Functional.Helpers;

/// <summary>
/// Manages Docker containers for Redis and Azurite used by functional tests.
/// Sets environment variables so the app and <see cref="ServiceFactAttribute"/>s pick them up.
/// </summary>
public static class DockerHelper
{
    private const string _redisContainerName = "oc-test-redis";
    private const string _azuriteContainerName = "oc-test-azurite";
    private const string _azuriteConnectionString =
        "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;"
        + "AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;"
        + "BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;";

    private static bool _dockerStarted;

    /// <summary>
    /// Starts Redis and Azurite Docker containers if Docker is available,
    /// removing any stale containers with the same names first.
    /// Sets the corresponding environment variables so the app and
    /// <see cref="ServiceFactAttribute"/>s pick them up.
    /// Containers are automatically removed when the test process exits.
    /// This method is safe to call multiple times — only the first call has effect.
    /// </summary>
    public static void TryStartDockerServices()
    {
        if (_dockerStarted)
        {
            return;
        }

        _dockerStarted = true;

        if (!IsDockerAvailable())
        {
            Log("Docker not available — skipping container startup.");
            return;
        }

        // Redis
        if (
            string.IsNullOrEmpty(
                System.Environment.GetEnvironmentVariable("OrchardCore__OrchardCore_Redis__Configuration")
            )
        )
        {
            RemoveContainer(_redisContainerName);
            RunDocker($"run -d --name {_redisContainerName} -p 6379:6379 redis:7");
            System.Environment.SetEnvironmentVariable(
                "OrchardCore__OrchardCore_Redis__Configuration",
                "localhost:6379"
            );
            Log($"Started Docker container '{_redisContainerName}' (Redis).");
        }

        // Azurite
        if (
            string.IsNullOrEmpty(
                System.Environment.GetEnvironmentVariable(
                    "OrchardCore__OrchardCore_Media_Azure__ConnectionString"
                )
            )
        )
        {
            RemoveContainer(_azuriteContainerName);
            RunDocker(
                $"run -d --name {_azuriteContainerName} -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite azurite --blobHost 0.0.0.0 --queueHost 0.0.0.0 --tableHost 0.0.0.0 --loose --skipApiVersionCheck"
            );
            System.Environment.SetEnvironmentVariable(
                "OrchardCore__OrchardCore_Media_Azure__ConnectionString",
                _azuriteConnectionString
            );
            System.Environment.SetEnvironmentVariable(
                "OrchardCore__OrchardCore_Media_Azure__ContainerName",
                "oc-media-tests"
            );
            Log($"Started Docker container '{_azuriteContainerName}' (Azurite).");
        }

        // Remove containers when the test process exits.
        AppDomain.CurrentDomain.ProcessExit += (_, _) => StopDockerServices();
    }

    /// <summary>
    /// Stops and removes the test Docker containers.
    /// </summary>
    public static void StopDockerServices()
    {
        if (!_dockerStarted)
        {
            return;
        }

        RemoveContainer(_redisContainerName);
        RemoveContainer(_azuriteContainerName);
        Log("Docker test containers removed.");
    }

    private static bool IsDockerAvailable()
    {
        try
        {
            // Use "docker version" instead of "docker info" — much less output, avoids buffer deadlocks.
            var (exitCode, _, _) = RunDocker("version", ignoreErrors: true);
            return exitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static void RemoveContainer(string name)
    {
        RunDocker($"rm -f {name}", ignoreErrors: true);
    }

    private static (int ExitCode, string StdOut, string StdErr) RunDocker(
        string arguments,
        bool ignoreErrors = false
    )
    {
        var psi = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi);
        if (process is null)
        {
            return (-1, string.Empty, "Failed to start docker process.");
        }

        // Read stdout/stderr asynchronously to avoid deadlocks from full buffers.
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();

        process.WaitForExit(60_000);

        var stdoutResult = stdout.GetAwaiter().GetResult();
        var stderrResult = stderr.GetAwaiter().GetResult();

        if (!ignoreErrors && process.ExitCode != 0)
        {
            Log($"Docker command failed: docker {arguments}\n{stderrResult}");
        }

        return (process.ExitCode, stdoutResult, stderrResult);
    }

    private static void Log(string message)
    {
        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] {message}");
    }
}

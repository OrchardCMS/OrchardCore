namespace OrchardCore.Tests.Functional.Helpers;

public static class AppLifecycleHelper
{
    private const string _dotnetVersion = "net10.0";

    public static void BuildApp(string appDir)
    {
        Log("Building application...");

        var process = Process.Start(
            new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build -c Release -f {_dotnetVersion}",
                WorkingDirectory = appDir,
                UseShellExecute = false,
            }
        );

        process?.WaitForExit();

        if (process?.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"dotnet build failed with exit code {process?.ExitCode}."
            );
        }

        Log("Build complete.");
    }

    public static void DeleteAppData(string appDir, string dataDir = "App_Data_Tests")
    {
        var fullPath = Path.Combine(appDir, dataDir);
        if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, recursive: true);
            Log($"{fullPath} deleted");
        }
    }

    public static bool CopyMigrationsRecipe(string appDir) =>
        CopyRecipe(appDir, "migrations.recipe.json");

    public static bool CopyRecipe(string appDir, string recipeFileName)
    {
        var destDir = Path.Combine(appDir, "Recipes");
        var destPath = Path.Combine(destDir, recipeFileName);

        if (File.Exists(destPath))
        {
            return false;
        }

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(recipeFileName, StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            return false;
        }

        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var fileStream = File.Create(destPath);
        stream!.CopyTo(fileStream);

        Log($"{recipeFileName} copied to {destDir}");

        return true;
    }

    public static void DeleteMigrationsRecipe(string appDir) =>
        DeleteRecipe(appDir, "migrations.recipe.json");

    public static void DeleteRecipe(string appDir, string recipeFileName)
    {
        var destDir = Path.Combine(appDir, "Recipes");
        var destPath = Path.Combine(destDir, recipeFileName);

        if (File.Exists(destPath))
        {
            File.Delete(destPath);
            Log($"{recipeFileName} deleted from {destDir}");
        }

        // Remove Recipes dir if empty.
        if (Directory.Exists(destDir) && !Directory.EnumerateFileSystemEntries(destDir).Any())
        {
            Directory.Delete(destDir);
        }
    }

    public static Process HostApp(string appDir, string assembly, string url = null)
    {
        var binPath = Path.Combine("bin", "Release", _dotnetVersion, assembly);
        var fullBinPath = Path.Combine(appDir, binPath);

        if (!File.Exists(fullBinPath))
        {
            BuildApp(appDir);
        }

        Log("Starting application...");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = binPath,
                WorkingDirectory = appDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            },
        };

        process.StartInfo.EnvironmentVariables["ORCHARD_APP_DATA"] = "./App_Data_Tests";

        if (!string.IsNullOrEmpty(url))
        {
            process.StartInfo.EnvironmentVariables["ASPNETCORE_URLS"] = url;
        }

        process.OutputDataReceived += (_, e) =>
        {
            if (
                !string.IsNullOrEmpty(e.Data)
                && (
                    e.Data.Contains("Exception")
                    || e.Data.StartsWith("fail:", StringComparison.Ordinal)
                )
            )
            {
                Console.Error.WriteLine($"[Server Error] {e.Data}");
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.Error.WriteLine($"[Server stderr] {e.Data}");
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return process;
    }

    public static async Task WaitForReadyAsync(string baseUrl, int timeoutMs = 60000)
    {
        var start = DateTime.UtcNow;
        Log($"Waiting for server at {baseUrl}...");

        using var client = new HttpClient();

        while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
        {
            try
            {
                var response = await client.GetAsync(baseUrl);
                var statusCode = (int)response.StatusCode;
                if (response.IsSuccessStatusCode || statusCode == 302 || statusCode == 404)
                {
                    Log("Server is ready.");
                    return;
                }
            }
            catch
            {
                // Server not ready yet.
            }

            await Task.Delay(1000);
        }

        throw new TimeoutException(
            $"Server at {baseUrl} did not become ready within {timeoutMs}ms."
        );
    }

    public static void KillApp(Process process)
    {
        if (process is not null && !process.HasExited)
        {
            process.Kill(entireProcessTree: true);
            Log("Server process killed.");
        }
    }

    // -- Docker container management for Redis / Azurite --

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
                Environment.GetEnvironmentVariable("OrchardCore__OrchardCore_Redis__Configuration")
            )
        )
        {
            RemoveContainer(_redisContainerName);
            RunDocker($"run -d --name {_redisContainerName} -p 6379:6379 redis:7");
            Environment.SetEnvironmentVariable(
                "OrchardCore__OrchardCore_Redis__Configuration",
                "localhost:6379"
            );
            Log($"Started Docker container '{_redisContainerName}' (Redis).");
        }

        // Azurite
        if (
            string.IsNullOrEmpty(
                Environment.GetEnvironmentVariable(
                    "OrchardCore__OrchardCore_Media_Azure__ConnectionString"
                )
            )
        )
        {
            RemoveContainer(_azuriteContainerName);
            RunDocker(
                $"run -d --name {_azuriteContainerName} -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite"
            );
            Environment.SetEnvironmentVariable(
                "OrchardCore__OrchardCore_Media_Azure__ConnectionString",
                _azuriteConnectionString
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

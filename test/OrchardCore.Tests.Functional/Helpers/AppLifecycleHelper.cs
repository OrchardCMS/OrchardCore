using System.Diagnostics;
using System.Reflection;

namespace OrchardCore.Tests.Functional.Helpers;

public static class AppLifecycleHelper
{
    private const string _dotnetVersion = "net10.0";

    public static void BuildApp(string appDir)
    {
        Log("Building application...");

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build -c Release -f {_dotnetVersion}",
            WorkingDirectory = appDir,
            UseShellExecute = false,
        });

        process?.WaitForExit();

        if (process?.ExitCode != 0)
        {
            throw new InvalidOperationException($"dotnet build failed with exit code {process?.ExitCode}.");
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

    public static bool CopyMigrationsRecipe(string appDir)
    {
        var recipeFileName = "migrations.recipe.json";
        var destDir = Path.Combine(appDir, "Recipes");
        var destPath = Path.Combine(destDir, recipeFileName);

        if (File.Exists(destPath))
        {
            return false;
        }

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
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

        Log($"Migrations recipe copied to {destDir}");

        return true;
    }

    public static void DeleteMigrationsRecipe(string appDir)
    {
        var destDir = Path.Combine(appDir, "Recipes");
        var destPath = Path.Combine(destDir, "migrations.recipe.json");

        if (File.Exists(destPath))
        {
            File.Delete(destPath);
            Log($"Migrations recipe deleted from {destDir}");
        }

        // Remove Recipes dir if empty.
        if (Directory.Exists(destDir) && !Directory.EnumerateFileSystemEntries(destDir).Any())
        {
            Directory.Delete(destDir);
        }
    }

    public static Process HostApp(string appDir, string assembly)
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

        process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data) &&
                (e.Data.Contains("Exception") || e.Data.StartsWith("fail:", StringComparison.Ordinal)))
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

        using var client = new HttpClient
        {
            // Use a short per-request timeout so the overall timeoutMs budget is respected.
            Timeout = TimeSpan.FromSeconds(5),
        };

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

        throw new TimeoutException($"Server at {baseUrl} did not become ready within {timeoutMs}ms.");
    }

    public static void KillApp(Process process)
    {
        if (process is not null && !process.HasExited)
        {
            process.Kill(entireProcessTree: true);
            Log("Server process killed.");
        }
    }

    private static void Log(string message)
    {
        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] {message}");
    }
}

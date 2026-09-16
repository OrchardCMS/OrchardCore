using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;

namespace OrchardCore.Cli;

internal static class DotnetEnvironment
{
    public static int RequiredMajor => new FrameworkName(typeof(Program).Assembly
        .GetCustomAttribute<TargetFrameworkAttribute>()!.FrameworkName).Version.Major;

    public static string PackageVersion => typeof(Program).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion.Split('+')[0];

    public static string? SelectSdk(string output) => output.Split('\n')
        .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault())
        .Where(value => Version.TryParse(value, out var version) && version.Major == RequiredMajor)
        .OrderByDescending(value => Version.Parse(value!))
        .FirstOrDefault();

    public static async Task<string?> FindSdkAsync(CancellationToken cancellationToken, IEnumerable<string>? secretEnvironmentVariables = null)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        try
        {
            var output = await RunAsync(["--list-sdks"], Path.GetTempPath(), null, null, timeout.Token, secretEnvironmentVariables);
            return SelectSdk(output);
        }
        catch (Exception exception) when (exception is CliException || exception is OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    internal static ProcessStartInfo CreateStartInfo(IEnumerable<string> arguments, string directory, IReadOnlyDictionary<string, string>? environment,
        IEnumerable<string>? secretEnvironmentVariables = null)
    {
        var info = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = directory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            CreateNoWindow = true,
        };
        foreach (var argument in arguments)
        {
            info.ArgumentList.Add(argument);
        }

        // A new local site must not inherit another application's tenant/database
        // configuration or extra Kestrel endpoints during loopback-only setup.
        foreach (var key in info.Environment.Keys.Where(key =>
            key.StartsWith("OrchardCore__", StringComparison.OrdinalIgnoreCase)
            || key.StartsWith("OrchardCore:", StringComparison.OrdinalIgnoreCase)
            || key.StartsWith("Kestrel__", StringComparison.OrdinalIgnoreCase)
            || key.StartsWith("Kestrel:", StringComparison.OrdinalIgnoreCase)
            || key.StartsWith("ASPNETCORE_Kestrel", StringComparison.OrdinalIgnoreCase)
            || key.StartsWith("DOTNET_Kestrel", StringComparison.OrdinalIgnoreCase)).ToArray())
        {
            info.Environment.Remove(key);
        }

        foreach (var key in secretEnvironmentVariables ?? [])
        {
            info.Environment.Remove(key);
        }

        info.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        info.Environment["DOTNET_NOLOGO"] = "1";
        if (environment is not null)
        {
            foreach (var pair in environment)
            {
                info.Environment[pair.Key] = pair.Value;
            }
        }

        return info;
    }

    public static Process Start(IEnumerable<string> arguments, string directory, IReadOnlyDictionary<string, string>? environment,
        IEnumerable<string>? secretEnvironmentVariables = null)
    {
        try
        {
            var process = Process.Start(CreateStartInfo(arguments, directory, environment, secretEnvironmentVariables))
                ?? throw new CliException("Could not start dotnet.");
            process.StandardInput.Close();
            return process;
        }
        catch (Win32Exception)
        {
            throw new CliException($"Install the .NET {RequiredMajor} SDK and make 'dotnet' available on PATH to use 'pomi install'.");
        }
    }

    public static async Task<string> RunAsync(IEnumerable<string> arguments, string directory, IReadOnlyDictionary<string, string>? environment,
        TextWriter? log, CancellationToken cancellationToken, IEnumerable<string>? secretEnvironmentVariables = null)
    {
        using var process = Start(arguments, directory, environment, secretEnvironmentVariables);
        var output = new StringBuilder();
        var stdout = DrainAsync(process.StandardOutput, log, output);
        var stderr = DrainAsync(process.StandardError, log);
        try
        {
            await process.WaitForExitAsync(cancellationToken);
            await Task.WhenAll(stdout, stderr);
            if (process.ExitCode != 0)
            {
                throw new CliException($"dotnet exited with code {process.ExitCode}. See the output above; the project was left in '{directory}' for inspection.");
            }

            return output.ToString();
        }
        finally
        {
            await StopAsync(process);
            await Task.WhenAll(stdout, stderr);
        }
    }

    public static async Task DrainAsync(StreamReader reader, TextWriter? log, StringBuilder? capture = null, string[]? secrets = null)
    {
        while (await reader.ReadLineAsync() is { } line)
        {
            foreach (var secret in secrets ?? [])
            {
                if (!string.IsNullOrEmpty(secret))
                {
                    line = line.Replace(secret, "[redacted]", StringComparison.Ordinal);
                }
            }

            if (capture is not null && capture.Length < 65536)
            {
                capture.AppendLine(line);
            }

            if (log is not null)
            {
                await log.WriteLineAsync(line);
            }
        }
    }

    public static async Task StopAsync(Process process)
    {
        if (!process.HasExited)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
                // The process exited between the check and the kill.
            }
        }

        await process.WaitForExitAsync(CancellationToken.None);
    }
}

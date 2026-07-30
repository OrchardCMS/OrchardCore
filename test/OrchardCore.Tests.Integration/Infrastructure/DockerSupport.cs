using System.Diagnostics;
using System.Runtime.CompilerServices;
using Xunit;

namespace OrchardCore.Tests.Integration.Infrastructure;

/// <summary>
/// Detects whether a usable Docker daemon is available so that Testcontainers-based
/// integration tests can be skipped gracefully on machines without Docker.
/// </summary>
internal static class DockerSupport
{
    private static readonly Lazy<bool> _isAvailable = new(Probe);

    /// <summary>
    /// Gets a value indicating whether a running Docker daemon could be reached.
    /// The result is probed once and cached for the lifetime of the test run.
    /// </summary>
    public static bool IsAvailable => _isAvailable.Value;

    private static bool Probe()
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "info --format \"{{.ServerVersion}}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            if (!process.Start())
            {
                return false;
            }

            if (!process.WaitForExit(TimeSpan.FromSeconds(15)))
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Best-effort: the probe already failed, nothing else to do.
                }

                return false;
            }

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// A <see cref="FactAttribute"/> that skips the test when no Docker daemon is available.
/// Testcontainers starts the required emulator automatically, so the only prerequisite
/// is a running Docker engine.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class DockerFactAttribute : FactAttribute
{
    public DockerFactAttribute(
        [CallerFilePath] string sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (!DockerSupport.IsAvailable)
        {
            Skip = "Docker is not available. Start Docker to run this integration test.";
        }
    }
}

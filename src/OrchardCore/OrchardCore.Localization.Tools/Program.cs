using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OrchardCore.Localization.Extraction;

namespace OrchardCore.Localization.Tools;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length != 2 || args[0] != "--request")
        {
            Console.Error.WriteLine("Usage: OrchardCore.Localization.Tools --request <request-file>");
            return 1;
        }

        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };
        try
        {
            var request = BuildRequest.Read(args[1]);
            var fingerprint = GetFingerprint(request, args[1], cancellation.Token);
            var stampPath = request.OutputPath + ".stamp.json";
            BuildStamp? stamp = null;
            if (File.Exists(stampPath) && File.Exists(request.OutputPath))
            {
                try
                {
                    stamp = JsonSerializer.Deserialize<BuildStamp>(await File.ReadAllTextAsync(stampPath, cancellation.Token));
                }
                catch (JsonException)
                {
                    // A truncated stamp is not a valid cache entry.
                }
            }

            if (stamp is not null && stamp.Fingerprint == fingerprint && stamp.CatalogHash == HashFile(request.OutputPath))
            {
                Report(stamp.Diagnostics);
                Console.WriteLine("Localization catalog is up to date.");
                return request.Options.Strict && stamp.Diagnostics.Length > 0 ? 1 : 0;
            }

            var catalog = LocalizationExtractor.Extract(request.Options, cancellation.Token);
            Report(catalog.Diagnostics);
            if (catalog.Diagnostics.Any(diagnostic => diagnostic.IsError) || request.Options.Strict && catalog.Diagnostics.Count > 0)
            {
                return 1;
            }

            await PotWriter.WriteIfChangedAsync(request.OutputPath, PotWriter.Write(catalog, request.Options.AssemblyName), cancellation.Token);
            stamp = new BuildStamp(fingerprint, HashFile(request.OutputPath), catalog.Diagnostics.ToArray());
            await PotWriter.WriteIfChangedAsync(stampPath, JsonSerializer.Serialize(stamp), cancellation.Token);
            Console.WriteLine("Localization catalog: " + catalog.Messages.Count.ToString(CultureInfo.InvariantCulture) + " messages.");
            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("error OCLOC009: Localization extraction was canceled.");
            return 1;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or FormatException or KeyNotFoundException or BadImageFormatException or InvalidOperationException)
        {
            Console.Error.WriteLine("error OCLOC009: " + OneLine(exception.Message));
            return 1;
        }
    }

    private static void Report(IEnumerable<ExtractionDiagnostic> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
        {
            var location = diagnostic.Source.Path.Length > 0 ? diagnostic.Source.Path + "(" + Math.Max(diagnostic.Source.Line, 1).ToString(CultureInfo.InvariantCulture) + ",1): " : "";
            Console.WriteLine(location + (diagnostic.IsError ? "error " : "warning ") + diagnostic.Code + ": " + OneLine(diagnostic.Message));
        }
    }

    private static string OneLine(string value) => value.Replace('\r', ' ').Replace('\n', ' ');

    private static string GetFingerprint(BuildRequest request, string requestPath, CancellationToken cancellationToken)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(File.ReadAllBytes(requestPath));
        var files = request.Options.Sources.Concat(request.Options.RazorFiles).Concat(request.Options.LiquidFiles).Select(file => file.Path)
            .Concat(request.Options.References)
            .Concat(request.Options.Analyzers)
            .Concat(Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll"))
            .Concat(Directory.EnumerateFiles(AppContext.BaseDirectory, "*.json"))
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal);
        foreach (var path in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            hash.AppendData(Encoding.UTF8.GetBytes(path));
            using var stream = File.OpenRead(path);
            hash.AppendData(SHA256.HashData(stream));
        }

        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private sealed record BuildStamp(string Fingerprint, string CatalogHash, ExtractionDiagnostic[] Diagnostics);
}

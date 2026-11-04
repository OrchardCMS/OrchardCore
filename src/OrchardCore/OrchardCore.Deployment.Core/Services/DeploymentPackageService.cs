using System.IO.Compression;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;

namespace OrchardCore.Deployment.Core.Services;

/// <summary>Validates and stages packages after the caller's file-creation pipeline.</summary>
public sealed class DeploymentPackageService
{
    private readonly ITempDirectoryProvider _temporary;
    private readonly DeploymentPackageOptions _options;

    /// <summary>Creates bounded package staging in the tenant's private temporary storage.</summary>
    public DeploymentPackageService(ITempDirectoryProvider temporary, IOptions<DeploymentPackageOptions> options)
    {
        _temporary = temporary;
        _options = options.Value;
    }

    /// <summary>Stages ZIP or JSON input without taking ownership of the input stream.</summary>
    public async Task<StagedDeploymentPackage> StageAsync(Stream input, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var extension = Path.GetExtension(fileName);
        if (!string.Equals(extension, ".zip", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Only ZIP and JSON deployment packages are supported.");
        }
        if (_options.MaxUploadBytes <= 0 || _options.MaxExpandedBytes <= 0 || _options.MaxEntries <= 0)
        {
            throw new InvalidOperationException("Deployment package limits must be positive.");
        }
        var folder = _temporary.CreateTempSubdirectory();
        var archivePath = folder + ".upload";
        var transferred = false;
        try
        {
            await using (var upload = new FileStream(archivePath, FileMode.CreateNew, FileAccess.Write))
            {
                await CopyBoundedAsync(input, upload, _options.MaxUploadBytes, cancellationToken);
            }
            if (string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase))
            {
                if (new FileInfo(archivePath).Length > _options.MaxExpandedBytes) { throw LimitExceeded(); }
                File.Move(archivePath, Path.Combine(folder, "Recipe.json"));
            }
            else
            {
                using var zip = ZipFile.OpenRead(archivePath);
                if (zip.Entries.Count > _options.MaxEntries) { throw LimitExceeded(); }
                var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                long expanded = 0;
                foreach (var entry in zip.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var directory = entry.FullName.EndsWith('/');
                    var path = directory ? entry.FullName[..^1] : entry.FullName;
                    if (string.IsNullOrEmpty(path) || path.Contains('\\') || path.Contains(':') || path.Any(char.IsControl)
                        || path.Split('/').Any(segment => segment is "" or "." or "..")
                        || !paths.Add(path) || ((entry.ExternalAttributes >> 16) & 0xF000) == 0xA000)
                    {
                        throw new InvalidDataException("Invalid or duplicate deployment archive path.");
                    }
                    var destination = Path.Combine(folder, path);
                    if (directory) { Directory.CreateDirectory(destination); continue; }
                    Directory.CreateDirectory(Path.GetDirectoryName(destination));
                    await using var source = entry.Open();
                    await using var target = new FileStream(destination, FileMode.CreateNew, FileAccess.Write);
                    expanded += await CopyBoundedAsync(source, target, _options.MaxExpandedBytes - expanded, cancellationToken);
                }
            }
            var recipePath = Path.Combine(folder, "Recipe.json");
            if (!File.Exists(recipePath)) { throw new InvalidDataException("A root Recipe.json is required."); }
            await using (var recipeStream = File.OpenRead(recipePath))
            {
                using var recipe = await JsonDocument.ParseAsync(recipeStream,
                    new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip }, cancellationToken);
                if (recipe.RootElement.ValueKind != JsonValueKind.Object
                    || !recipe.RootElement.TryGetProperty("steps", out var steps) || steps.ValueKind != JsonValueKind.Array
                    || steps.EnumerateArray().Any(step => step.ValueKind != JsonValueKind.Object
                        || !step.TryGetProperty("name", out var name) || name.ValueKind != JsonValueKind.String
                        || string.IsNullOrWhiteSpace(name.GetString())))
                {
                    throw new InvalidDataException("A deployment recipe requires an array of named step objects.");
                }
            }
            var package = new StagedDeploymentPackage(folder, File.Exists(archivePath) ? archivePath : recipePath);
            transferred = true;
            return package;
        }
        catch
        {
            Directory.Delete(folder, recursive: true);
            throw;
        }
        finally
        {
            if (!transferred) { File.Delete(archivePath); }
        }
    }

    private static async Task<long> CopyBoundedAsync(Stream source, FileStream target, long limit, CancellationToken cancellationToken)
    {
        var buffer = new byte[81920];
        long length = 0;
        int read;
        while ((read = await source.ReadAsync(buffer, cancellationToken)) != 0)
        {
            if (read > limit - length) { throw LimitExceeded(); }
            await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            length += read;
        }
        return length;
    }

    private static InvalidDataException LimitExceeded() => new("The deployment package exceeds the configured size or entry limit.");
}

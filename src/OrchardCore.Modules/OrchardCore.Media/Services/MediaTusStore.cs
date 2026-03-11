using System.IO.Pipelines;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using tusdotnet.Interfaces;
using tusdotnet.Models;

namespace OrchardCore.Media.Services;

/// <summary>
/// A custom TUS store that buffers partial uploads on local disk and moves
/// completed files into <see cref="IMediaFileStore"/> on completion.
/// </summary>
public sealed class MediaTusStore :
    ITusStore,
    ITusPipelineStore,
    ITusCreationStore,
    ITusTerminationStore,
    ITusExpirationStore
{
    private const string TusTempFolderName = "TusUploads";
    private const string MetadataExtension = ".metadata";
    private const string ExpirationExtension = ".expiration";
    private const string LengthExtension = ".length";
    private const string OffsetExtension = ".offset";

    private readonly string _tempDirectory;
    private readonly ILogger _logger;
    private readonly IClock _clock;

    public MediaTusStore(
        ShellSettings shellSettings,
        IClock clock,
        ILogger<MediaTusStore> logger)
    {
        _clock = clock;
        _logger = logger;

        _tempDirectory = Path.Combine(Path.GetTempPath(), TusTempFolderName, shellSettings.TenantId);
    }

    // ITusCreationStore

    public async Task<string> CreateFileAsync(long uploadLength, string metadata, CancellationToken cancellationToken)
    {
        var fileId = Guid.NewGuid().ToString("N");
        EnsureTempDirectory();

        // Create empty data file (do not pre-allocate with SetLength, as the
        // actual file size is used to track bytes written on cancellation).
        var dataPath = GetDataPath(fileId);
        await using (var fs = new FileStream(dataPath, FileMode.CreateNew, FileAccess.Write))
        {
        }

        // Store metadata.
        if (!string.IsNullOrEmpty(metadata))
        {
            await File.WriteAllTextAsync(GetMetadataPath(fileId), metadata, cancellationToken);
        }

        // Store declared length.
        await File.WriteAllTextAsync(GetLengthPath(fileId), uploadLength.ToString(), cancellationToken);

        // Initialize offset to 0.
        await File.WriteAllTextAsync(GetOffsetPath(fileId), "0", cancellationToken);

        return fileId;
    }

    public Task<string> GetUploadMetadataAsync(string fileId, CancellationToken cancellationToken)
    {
        var path = GetMetadataPath(fileId);
        return Task.FromResult(File.Exists(path) ? File.ReadAllText(path) : null);
    }

    // ITusStore

    public Task<bool> FileExistAsync(string fileId, CancellationToken cancellationToken)
    {
        return Task.FromResult(File.Exists(GetDataPath(fileId)));
    }

    public Task<long?> GetUploadLengthAsync(string fileId, CancellationToken cancellationToken)
    {
        var path = GetLengthPath(fileId);
        if (!File.Exists(path))
        {
            return Task.FromResult<long?>(null);
        }

        // Sidecar files are tiny; read synchronously to avoid TaskCanceledException
        // when the client disconnects mid-upload (e.g., browser refresh).
        var text = File.ReadAllText(path);
        long? result = long.TryParse(text, out var length) ? length : null;
        return Task.FromResult(result);
    }

    public Task<long> GetUploadOffsetAsync(string fileId, CancellationToken cancellationToken)
    {
        var path = GetOffsetPath(fileId);
        if (!File.Exists(path))
        {
            return Task.FromResult(0L);
        }

        var text = File.ReadAllText(path);
        return Task.FromResult(long.TryParse(text, out var offset) ? offset : 0L);
    }

    public async Task<long> AppendDataAsync(string fileId, Stream stream, CancellationToken cancellationToken)
    {
        var offset = await GetUploadOffsetAsync(fileId, cancellationToken);
        var dataPath = GetDataPath(fileId);

        long bytesWritten = 0;
        var fs = OpenDataFileForWrite(dataPath);
        try
        {
            fs.Seek(offset, SeekOrigin.Begin);
            await stream.CopyToAsync(fs, cancellationToken);
            bytesWritten = fs.Position - offset;
        }
        catch (OperationCanceledException)
        {
            // Client disconnected (pause or browser refresh). Flush buffered data
            // and read the stream position to persist correct progress.
            try { fs.Flush(); } catch { /* best effort */ }
            bytesWritten = fs.Position - offset;
            if (bytesWritten < 0)
            {
                bytesWritten = 0;
            }
        }
        finally
        {
            await fs.DisposeAsync();
        }

        var newOffset = offset + bytesWritten;
        File.WriteAllText(GetOffsetPath(fileId), newOffset.ToString());

        return bytesWritten;
    }

    // ITusPipelineStore

    public async Task<long> AppendDataAsync(string fileId, PipeReader pipeReader, CancellationToken cancellationToken)
    {
        var offset = await GetUploadOffsetAsync(fileId, cancellationToken);
        var dataPath = GetDataPath(fileId);

        long bytesWritten = 0;
        var fs = OpenDataFileForWrite(dataPath);
        try
        {
            fs.Seek(offset, SeekOrigin.Begin);

            while (true)
            {
                var result = await pipeReader.ReadAsync(cancellationToken);
                var buffer = result.Buffer;

                foreach (var segment in buffer)
                {
                    await fs.WriteAsync(segment, cancellationToken);
                    bytesWritten += segment.Length;
                }

                pipeReader.AdvanceTo(buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected (pause or browser refresh). Flush buffered data
            // and use stream position as the most accurate byte count.
            try { fs.Flush(); } catch { /* best effort */ }
            bytesWritten = fs.Position - offset;
            if (bytesWritten < 0)
            {
                bytesWritten = 0;
            }
        }
        finally
        {
            await fs.DisposeAsync();
        }

        var newOffset = offset + bytesWritten;
        File.WriteAllText(GetOffsetPath(fileId), newOffset.ToString());

        return bytesWritten;
    }

    // ITusTerminationStore

    public Task DeleteFileAsync(string fileId, CancellationToken cancellationToken)
    {
        DeleteFileAndMetadata(fileId);
        return Task.CompletedTask;
    }

    // ITusExpirationStore

    public async Task SetExpirationAsync(string fileId, DateTimeOffset expires, CancellationToken cancellationToken)
    {
        await File.WriteAllTextAsync(
            GetExpirationPath(fileId),
            expires.UtcTicks.ToString(),
            cancellationToken);
    }

    public Task<DateTimeOffset?> GetExpirationAsync(
        string fileId,
        CancellationToken cancellationToken
    )
    {
        var path = GetExpirationPath(fileId);
        if (!File.Exists(path))
        {
            return Task.FromResult<DateTimeOffset?>(null);
        }

        var text = File.ReadAllText(path);
        DateTimeOffset? result = long.TryParse(text, out var ticks)
            ? new DateTimeOffset(ticks, TimeSpan.Zero)
            : null;
        return Task.FromResult(result);
    }

    public Task<IEnumerable<string>> GetExpiredFilesAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_tempDirectory))
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        var now = _clock.UtcNow;
        var expired = new List<string>();

        foreach (var expirationFile in Directory.GetFiles(_tempDirectory, $"*{ExpirationExtension}"))
        {
            var text = File.ReadAllText(expirationFile);
            if (long.TryParse(text, out var ticks) && new DateTimeOffset(ticks, TimeSpan.Zero) < now)
            {
                var fileId = Path.GetFileNameWithoutExtension(expirationFile);

                // Only return files that are incomplete (offset < length).
                var dataPath = GetDataPath(fileId);
                if (File.Exists(dataPath))
                {
                    expired.Add(fileId);
                }
            }
        }

        return Task.FromResult<IEnumerable<string>>(expired);
    }

    public Task<int> RemoveExpiredFilesAsync(CancellationToken cancellationToken)
    {
        var expiredTask = GetExpiredFilesAsync(cancellationToken);
        // GetExpiredFilesAsync is synchronous in our implementation, so .Result is safe here.
        var expiredFiles = expiredTask.Result.ToList();

        foreach (var fileId in expiredFiles)
        {
            DeleteFileAndMetadata(fileId);
        }

        if (expiredFiles.Count > 0 && _logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Removed {Count} expired TUS upload(s).", expiredFiles.Count);
        }

        return Task.FromResult(expiredFiles.Count);
    }

    // Public helpers

    /// <summary>
    /// Opens the completed upload's data file for reading.
    /// Caller is responsible for disposing the stream.
    /// </summary>
    public Stream OpenReadStream(string fileId)
    {
        var dataPath = GetDataPath(fileId);
        return new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    /// <summary>
    /// Gets the file name from the data file.
    /// </summary>
    public long GetFileLength(string fileId)
    {
        var dataPath = GetDataPath(fileId);
        return new FileInfo(dataPath).Length;
    }

    // Private helpers

    /// <summary>
    /// Opens the data file for writing with <see cref="FileShare.ReadWrite"/> so that
    /// a new PATCH request can open the file even if a previous (aborted) request's
    /// FileStream has not yet been fully disposed. The cancelled stream will not write
    /// any further data, and the new stream seeks to the correct offset from the
    /// sidecar file, so concurrent handles are safe here.
    /// </summary>
    private static FileStream OpenDataFileForWrite(string path)
    {
        return new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
    }

    private void EnsureTempDirectory()
    {
        if (!Directory.Exists(_tempDirectory))
        {
            Directory.CreateDirectory(_tempDirectory);
        }
    }

    private string GetDataPath(string fileId) =>
        Path.Combine(_tempDirectory, fileId);

    private string GetMetadataPath(string fileId) =>
        Path.Combine(_tempDirectory, fileId + MetadataExtension);

    private string GetLengthPath(string fileId) =>
        Path.Combine(_tempDirectory, fileId + LengthExtension);

    private string GetOffsetPath(string fileId) =>
        Path.Combine(_tempDirectory, fileId + OffsetExtension);

    private string GetExpirationPath(string fileId) =>
        Path.Combine(_tempDirectory, fileId + ExpirationExtension);

    private void DeleteFileAndMetadata(string fileId)
    {
        TryDelete(GetDataPath(fileId));
        TryDelete(GetMetadataPath(fileId));
        TryDelete(GetLengthPath(fileId));
        TryDelete(GetOffsetPath(fileId));
        TryDelete(GetExpirationPath(fileId));
    }

    private void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete TUS temp file '{Path}'.", path);
        }
    }
}

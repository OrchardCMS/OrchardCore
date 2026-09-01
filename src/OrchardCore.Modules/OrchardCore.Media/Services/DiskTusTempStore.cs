using System.IO.Pipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Media.Services;

/// <summary>
/// Default <see cref="ITusTempStore"/> implementation that stores partial upload
/// data on disk. The base path is configurable via <see cref="MediaOptions.TusTempPath"/>
/// (defaults to <c>Path.GetTempPath()/TusUploads</c>).
/// Configure this to a shared filesystem path for multi-instance deployments.
/// </summary>
public sealed class DiskTusTempStore : ITusTempStore
{
    private readonly string _tempDirectory;
    private readonly ILogger _logger;

    public DiskTusTempStore(
        IOptions<MediaOptions> mediaOptions,
        ShellSettings shellSettings,
        ILogger<DiskTusTempStore> logger)
    {
        _logger = logger;
        _tempDirectory = Path.Combine(mediaOptions.Value.TusTempPath, shellSettings.TenantId);
    }

    public Task CreateFileAsync(string fileId, CancellationToken cancellationToken)
    {
        EnsureTempDirectory();

        var dataPath = GetDataPath(fileId);
        using (new FileStream(dataPath, FileMode.CreateNew, FileAccess.Write))
        {
        }

        return Task.CompletedTask;
    }

    public async Task<long> AppendDataAsync(string fileId, Stream stream, long offset, CancellationToken cancellationToken)
    {
        var dataPath = GetDataPath(fileId);

        long bytesWritten = 0;
        var fs = OpenDataFileForWrite(dataPath);
        try
        {
            fs.Seek(offset, SeekOrigin.Begin);
            await stream.CopyToAsync(fs, cancellationToken);
            bytesWritten = fs.Position - offset;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            bytesWritten = fs.Position - offset;
            if (bytesWritten < 0)
            {
                bytesWritten = 0;
            }
        }
        catch (BadHttpRequestException exception)
            when (ClientDisconnectAwarePipeReader.IsUnexpectedEndOfRequest(exception))
        {
            bytesWritten = Math.Max(0, fs.Position - offset);
        }
        finally
        {
            await fs.DisposeAsync();
        }

        return bytesWritten;
    }

    public async Task<long> AppendDataAsync(string fileId, PipeReader pipeReader, long offset, CancellationToken cancellationToken)
    {
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

                if (result.IsCanceled || result.IsCompleted)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            bytesWritten = fs.Position - offset;
            if (bytesWritten < 0)
            {
                bytesWritten = 0;
            }
        }
        catch (BadHttpRequestException exception)
            when (ClientDisconnectAwarePipeReader.IsUnexpectedEndOfRequest(exception))
        {
            bytesWritten = Math.Max(0, fs.Position - offset);
        }
        finally
        {
            try
            {
                await pipeReader.CompleteAsync();
            }
            finally
            {
                await fs.DisposeAsync();
            }
        }

        return bytesWritten;
    }

    public Task DeleteFileAsync(string fileId, CancellationToken cancellationToken)
    {
        TryDelete(GetDataPath(fileId));
        return Task.CompletedTask;
    }

    public Task<bool> FileExistAsync(string fileId, CancellationToken cancellationToken)
    {
        return Task.FromResult(File.Exists(GetDataPath(fileId)));
    }

    public Stream OpenReadStream(string fileId)
    {
        var dataPath = GetDataPath(fileId);
        return new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public long GetFileLength(string fileId)
    {
        var dataPath = GetDataPath(fileId);
        return new FileInfo(dataPath).Length;
    }

    /// <summary>
    /// Opens the data file for writing with <see cref="FileShare.ReadWrite"/> so that
    /// a new PATCH request can open the file even if a previous (aborted) request's
    /// FileStream has not yet been fully disposed.
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

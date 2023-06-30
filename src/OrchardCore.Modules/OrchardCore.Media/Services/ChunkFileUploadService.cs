using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;

namespace OrchardCore.Media.Services;

public class ChunkFileUploadService : IChunkFileUploadService
{
    private const String UploadIdFormKey = "__chunkedFileUploadId";
    private const String TempFolderPrefix = "ChunkedFileUploads";
    private static readonly SemaphoreSlim _purgeLock = new(1, 1);
    private readonly IOptions<MediaOptions> _options;
    private readonly ILogger _logger;
    private readonly IClock _clock;

    public ChunkFileUploadService(
        IClock clock,
        ILogger<ChunkFileUploadService> logger,
        IOptions<MediaOptions> options)
    {
        _clock = clock;
        _logger = logger;
        _options = options;
    }

    public async Task<IActionResult> ProcessRequestAsync(
        HttpRequest request,
        Func<Guid, IFormFile, ContentRangeHeaderValue, Task<IActionResult>> chunkAsync,
        Func<IEnumerable<IFormFile>, Task<IActionResult>> completedAsync)
    {
        var contentRangeHeader = request.Headers.ContentRange;

        if (_options.Value.MaxUploadChunkSize is null or <= 0
            || contentRangeHeader.Count is 0
            || !request.Form.TryGetValue(UploadIdFormKey, out var uploadIdValue))
        {
            return await completedAsync(request.Form.Files);
        }

        if (request.Form.Files.Count != 1
            || !ContentRangeHeaderValue.TryParse(contentRangeHeader, out var contentRange)
            || !Guid.TryParse(uploadIdValue, out var uploadId)
            || !contentRange.HasLength
            || !contentRange.HasRange
            || contentRange.Length > _options.Value.MaxFileSize
            || contentRange.To - contentRange.From > _options.Value.MaxUploadChunkSize)
        {
            return new BadRequestResult();
        }

        var formFile = request.Form.Files.First();
        using (var fileStream = GetOrCreateTemporaryFile(
            uploadId,
            formFile,
            contentRange.Length.Value))
        {
            fileStream.Seek(contentRange.From.Value, SeekOrigin.Begin);
            await formFile.CopyToAsync(fileStream, request.HttpContext.RequestAborted);
        }

        return (contentRange.To.Value + 1) >= contentRange.Length.Value
            ? await CompleteUploadAsync(uploadId, formFile, completedAsync)
            : await chunkAsync(uploadId, formFile, contentRange);
    }

    private Stream GetOrCreateTemporaryFile(Guid uploadId, IFormFile formFile, long size)
    {
        var siteTempFolderPath = GetTempFolderPath();

        if (!Directory.Exists(siteTempFolderPath))
        {
            Directory.CreateDirectory(siteTempFolderPath);
        }

        var tempFilePath = GetTempFilePath(uploadId, formFile);

        return File.Exists(tempFilePath) switch
        {
            true => File.Open(tempFilePath, FileMode.Open, FileAccess.ReadWrite),
            false => CreateTemporaryFile(tempFilePath, size),
        };
    }

    private async Task<IActionResult> CompleteUploadAsync(
        Guid uploadId,
        IFormFile formFile,
        Func<IEnumerable<IFormFile>, Task<IActionResult>> completedAsync)
    {
        var chunkedFormFile = GetTemporaryFileForRead(uploadId, formFile);

        try
        {
            return await completedAsync(new[] { chunkedFormFile });
        }
        finally
        {
            chunkedFormFile.Dispose();
            DeleteTemporaryFile(uploadId, formFile);
            PurgeTempDirectory();
        }
    }

    private void PurgeTempDirectory()
    {
        if (_purgeLock.CurrentCount == 0)
        {
            return;
        }

        _purgeLock.Wait();

        try
        {
            var siteTempFolderPath = GetTempFolderPath();
            if (_options.Value.TemporaryFileLifetime <= TimeSpan.Zero
                || !Directory.Exists(siteTempFolderPath))
            {
                return;
            }

            var tempFiles = Directory.GetFiles(siteTempFolderPath)
                .Select(filePath => new FileInfo(filePath))
                .Where(fileInfo => fileInfo.LastWriteTimeUtc + _options.Value.TemporaryFileLifetime < _clock.UtcNow)
                .ToList();

            foreach (var tempFile in tempFiles)
            {
                DeleteTemporaryFile(tempFile.FullName);
            }
        }
        finally
        {
            _purgeLock.Release();
        }
    }

    private void DeleteTemporaryFile(Guid uploadId, IFormFile formFile) =>
        DeleteTemporaryFile(GetTempFilePath(uploadId, formFile));

    private void DeleteTemporaryFile(string tempFilePath)
    {
        try
        {
            File.Delete(tempFilePath);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "An error occurred while deleting the temporary file '{TempFilePath}'.",
                tempFilePath);
        }
    }

    private static ChunkedFormFile GetTemporaryFileForRead(Guid uploadId, IFormFile formFile)
    {
        var tempFilePath = GetTempFilePath(uploadId, formFile);

        return new(File.OpenRead(tempFilePath))
        {
            ContentType = formFile.ContentType,
            ContentDisposition = formFile.ContentDisposition,
            Headers = formFile.Headers,
            Name = formFile.Name,
            FileName = formFile.FileName,
        };
    }

    private static string GetTempFolderPath()
    {
        var siteTempFolderPath = Path.Combine(Path.GetTempPath(), TempFolderPrefix);

        return siteTempFolderPath;
    }

    private static string GetTempFilePath(Guid uploadId, IFormFile formFile)
    {
        var siteTempFolderPath = GetTempFolderPath();
        var tempFilePath = Path.Combine(
            siteTempFolderPath,
            CalculateHash(uploadId.ToString(), formFile.FileName, formFile.Name));

        return tempFilePath;
    }

    private static Stream CreateTemporaryFile(string tempPath, long size)
    {
        var stream = File.Create(tempPath);
        stream.SetLength(size);

        var tempFileInfo = new FileInfo(tempPath);
        tempFileInfo.Attributes |= FileAttributes.Temporary;

        return stream;
    }

    private static string CalculateHash(params string[] parts)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(String.Join(String.Empty, parts)));

        return Convert.ToHexString(hash);
    }
}

internal sealed class ChunkedFormFile : IFormFile, IDisposable
{
    private readonly Stream _stream;
    private bool _disposed;

    public string ContentType { get; set; }

    public string ContentDisposition { get; set; }

    public IHeaderDictionary Headers { get; set; }

    public long Length => _stream.Length;

    public string Name { get; set; }

    public string FileName { get; set; }

    public ChunkedFormFile(Stream stream) =>
        _stream = stream;

    public void CopyTo(Stream target) =>
        _stream.CopyTo(target);

    public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default) =>
        _stream.CopyToAsync(target, cancellationToken);

    public Stream OpenReadStream() =>
        _stream;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _stream?.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose() =>
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
}

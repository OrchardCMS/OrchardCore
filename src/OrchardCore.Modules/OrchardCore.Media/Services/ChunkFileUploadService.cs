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
using OrchardCore.Settings;

namespace OrchardCore.Media.Services;

public class ChunkFileUploadService : IChunkFileUploadService
{
    private const String UploadIdFormKey = "__chunkedFileUploadId";
    private const String TempFolderPrefix = "ChunkedFileUploads";
    private readonly IOptions<MediaOptions> _options;
    private readonly ISiteService _siteService;
    private readonly ILogger _logger;
    private readonly IClock _clock;

    public ChunkFileUploadService(
        ISiteService siteService,
        IClock clock,
        ILogger<ChunkFileUploadService> logger,
        IOptions<MediaOptions> options)
    {
        _siteService = siteService;
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
            || contentRange.Length > _options.Value.MaxFileSize)
        {
            return new BadRequestResult();
        }

        var formFile = request.Form.Files.First();
        using (var fileStream = await GetOrCreateTemporaryFileAsync(
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

    public async Task PurgeTempDirectoryAsync()
    {
        var siteTempFolderPath = await GetSiteTempFolderPathAsync();
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

    private async Task<Stream> GetOrCreateTemporaryFileAsync(Guid uploadId, IFormFile formFile, long size)
    {
        var siteTempFolderPath = await GetSiteTempFolderPathAsync();

        if (!Directory.Exists(siteTempFolderPath))
        {
            Directory.CreateDirectory(siteTempFolderPath);
        }

        var tempFilePath = await GetTempFilePathAsync(uploadId, formFile);

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
        var chunkedFormFile = await GetTemporaryFileForReadAsync(uploadId, formFile);

        try
        {
            return await completedAsync(new[] { chunkedFormFile });
        }
        finally
        {
            chunkedFormFile.Dispose();
            await DeleteTemporaryFileAsync(uploadId, formFile);
        }
    }

    private async Task<ChunkedFormFile> GetTemporaryFileForReadAsync(Guid uploadId, IFormFile formFile)
    {
        var tempFilePath = await GetTempFilePathAsync(uploadId, formFile);

        return new(File.OpenRead(tempFilePath))
        {
            ContentType = formFile.ContentType,
            ContentDisposition = formFile.ContentDisposition,
            Headers = formFile.Headers,
            Name = formFile.Name,
            FileName = formFile.FileName,
        };
    }

    private async Task DeleteTemporaryFileAsync(Guid uploadId, IFormFile formFile) =>
        DeleteTemporaryFile(await GetTempFilePathAsync(uploadId, formFile));

    private void DeleteTemporaryFile(String tempFilePath)
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

    private async Task<String> GetSiteTempFolderPathAsync()
    {
        var siteName = (await _siteService.GetSiteSettingsAsync()).SiteName;
        var siteTempFolderPath = Path.Combine(Path.GetTempPath(), siteName, TempFolderPrefix);

        return siteTempFolderPath;
    }

    private async Task<String> GetTempFilePathAsync(Guid uploadId, IFormFile formFile)
    {
        var siteTempFolderPath = await GetSiteTempFolderPathAsync();
        var tempFilePath = Path.Combine(
            siteTempFolderPath,
            CalculateHash(uploadId.ToString(), formFile.FileName, formFile.Name));

        return tempFilePath;
    }

    private static Stream CreateTemporaryFile(String tempPath, long size)
    {
        var stream = File.Create(tempPath);
        stream.SetLength(size);

        var tempFileInfo = new FileInfo(tempPath);
        tempFileInfo.Attributes |= FileAttributes.Temporary;

        return stream;
    }

    private static String CalculateHash(params String[] parts)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(String.Join(String.Empty, parts)));

        return BitConverter.ToString(hash).Replace("-", String.Empty);
    }
}

internal sealed class ChunkedFormFile : IFormFile, IDisposable
{
    private readonly Stream _stream;
    private bool _disposed;

    public String ContentType { get; set; }

    public String ContentDisposition { get; set; }

    public IHeaderDictionary Headers { get; set; }

    public long Length => _stream.Length;

    public String Name { get; set; }

    public String FileName { get; set; }

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

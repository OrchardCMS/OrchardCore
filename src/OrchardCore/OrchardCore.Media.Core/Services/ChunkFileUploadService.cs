using System.IO.Hashing;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.FileStorage;
using OrchardCore.Modules;

namespace OrchardCore.Media.Services;

public sealed class ChunkFileUploadService : IChunkFileUploadService
{
    private const string s_uploadIdFormKey = "__chunkedFileUploadId";
    private const string s_tempFolderName = "ChunkedFileUploads";

    private readonly IClock _clock;
    private readonly ILogger _logger;
    private readonly IOptions<MediaOptions> _options;
    private readonly ITempDirectoryProvider _tempDirectoryProvider;
    private readonly string _tempFileNamePrefix;

    public ChunkFileUploadService(
        ShellSettings shellSettings,
        IClock clock,
        ILogger<ChunkFileUploadService> logger,
        IOptions<MediaOptions> options,
        ITempDirectoryProvider tempDirectoryProvider)
    {
        _clock = clock;
        _logger = logger;
        _options = options;
        _tempDirectoryProvider = tempDirectoryProvider;

        _tempFileNamePrefix = $"{shellSettings.TenantId}_";
    }

    public async Task<IActionResult> ProcessRequestAsync(
        HttpRequest request,
        Func<Guid, IFormFile, ContentRangeHeaderValue, Task<IActionResult>> chunkAsync,
        Func<IEnumerable<IFormFile>, Task<IActionResult>> completedAsync)
    {
        var contentRangeHeader = request.Headers.ContentRange;

        if (_options.Value.MaxUploadChunkSize <= 0 ||
            contentRangeHeader.Count is 0 ||
            !request.Form.TryGetValue(s_uploadIdFormKey, out var uploadIdValue))
        {
            return await completedAsync(request.Form.Files);
        }

        if (request.Form.Files.Count != 1 ||
            !ContentRangeHeaderValue.TryParse(contentRangeHeader, out var contentRange) ||
            !Guid.TryParse(uploadIdValue, out var uploadId) ||
            !contentRange.HasLength ||
            !contentRange.HasRange ||
            contentRange.Length > _options.Value.MaxFileSize ||
            contentRange.To - contentRange.From > _options.Value.MaxUploadChunkSize)
        {
            return new BadRequestResult();
        }

        var formFile = request.Form.Files[0];
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

    private async Task<IActionResult> CompleteUploadAsync(
        Guid uploadId,
        IFormFile formFile,
        Func<IEnumerable<IFormFile>, Task<IActionResult>> completedAsync)
    {
        try
        {
            using var chunkedFormFile = GetTemporaryFileForRead(uploadId, formFile);

            return await completedAsync(new[] { chunkedFormFile });
        }
        finally
        {
            DeleteTemporaryFile(uploadId, formFile);
        }
    }

    public void PurgeTempDirectory()
    {
        var tempFolderPath = GetTempFolderPath();
        if (_options.Value.TemporaryFileLifetime <= TimeSpan.Zero ||
            !Directory.Exists(tempFolderPath))
        {
            return;
        }

        var tempFiles = Directory.GetFiles(tempFolderPath, $"{_tempFileNamePrefix}*")
            .Select(filePath => new FileInfo(filePath))
            .Where(fileInfo => fileInfo.LastWriteTimeUtc + _options.Value.TemporaryFileLifetime < _clock.UtcNow)
            .ToArray();

        foreach (var tempFile in tempFiles)
        {
            if (!DeleteTemporaryFile(tempFile.FullName))
            {
                break;
            }
        }
    }

    private bool DeleteTemporaryFile(Guid uploadId, IFormFile formFile) =>
        DeleteTemporaryFile(GetTempFilePath(uploadId, formFile));

    private bool DeleteTemporaryFile(string tempFilePath)
    {
        try
        {
            File.Delete(tempFilePath);

            return true;
        }
        catch (Exception exception) when (exception.IsFileSharingViolation())
        {
            _logger.LogError(
                exception,
                "Sharing violation while deleting the temporary file '{TempFilePath}'.",
                tempFilePath);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "An error occurred while deleting the temporary file '{TempFilePath}'.",
                tempFilePath);
        }

        return false;
    }

    private FileStream GetOrCreateTemporaryFile(Guid uploadId, IFormFile formFile, long size)
    {
        // GetTempFilePath resolves the tenant-scoped temp folder via ITempDirectoryProvider, which ensures it exists.
        var tempFilePath = GetTempFilePath(uploadId, formFile);

        return File.Exists(tempFilePath) switch
        {
            true => File.Open(tempFilePath, FileMode.Open, FileAccess.ReadWrite),
            false => CreateTemporaryFile(tempFilePath, size),
        };
    }

    private ChunkedFormFile GetTemporaryFileForRead(Guid uploadId, IFormFile formFile)
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

    private string GetTempFolderPath()
    {
        var tempFolderPath = Path.Combine(_tempDirectoryProvider.GetRootDirectory(), s_tempFolderName);

        Directory.CreateDirectory(tempFolderPath);

        return tempFolderPath;
    }

    private string GetTempFilePath(Guid uploadId, IFormFile formFile) =>
        Path.Combine(
            GetTempFolderPath(),
            $"{_tempFileNamePrefix}{CalculateHash(uploadId.ToString(), formFile.FileName, formFile.Name)}");

    private static FileStream CreateTemporaryFile(string tempPath, long size)
    {
        var stream = File.Create(tempPath);
        stream.SetLength(size);

        var tempFileInfo = new FileInfo(tempPath);
        tempFileInfo.Attributes |= FileAttributes.Temporary;

        return stream;
    }

    private static string CalculateHash(params string[] parts)
    {
        var hash = new XxHash64();

        foreach (var part in parts)
        {
            hash.Append(MemoryMarshal.AsBytes<char>(part));
        }

        return Convert.ToHexString(hash.GetCurrentHash());
    }

    private sealed class ChunkedFormFile : IFormFile, IDisposable
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

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            _stream?.Dispose();
        }
    }
}

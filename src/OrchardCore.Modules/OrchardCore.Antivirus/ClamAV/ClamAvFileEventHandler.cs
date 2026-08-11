using System.Net.Sockets;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Infrastructure;

namespace OrchardCore.Antivirus.ClamAV;

internal sealed class ClamAvFileEventHandler : IFileEventHandler
{
    private readonly ClamAvOptions _options;
    private readonly ClamAvConnectionFactory _connectionFactory;
    private readonly ILogger _logger;

    public ClamAvFileEventHandler(
        IOptions<ClamAvOptions> options,
        ClamAvConnectionFactory connectionFactory,
        ILogger<ClamAvFileEventHandler> logger)
    {
        _options = options.Value;
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<FileCreatingResult> CreatingAsync(FileCreatingContext context, Stream stream, CancellationToken cancellationToken = default)
    {
        ValidateOptions();

        var scanStream = stream;

        if (scanStream.CanSeek)
        {
            scanStream.Position = 0;
        }
        else
        {
            scanStream = await CreateSeekableStreamAsync(scanStream, cancellationToken);
        }

        try
        {
            var response = await _connectionFactory.Create(_options).ScanAsync(scanStream, cancellationToken);

            if (TryCreateFailureResult(context, scanStream, response) is { } failureResult)
            {
                return failureResult;
            }

            scanStream.Position = 0;

            return FileCreatingResult.Success(scanStream);
        }
        catch (OperationCanceledException exception)
        {
            if (scanStream != stream)
            {
                await scanStream.DisposeAsync();
            }

            _logger.LogError(exception, "ClamAV timed out while scanning '{FileName}'.", context.FileName);

            throw new AntivirusScanningException($"The ClamAV antivirus scanner timed out while scanning '{context.FileName}'.", exception);
        }
        catch (SocketException exception)
        {
            if (scanStream != stream)
            {
                await scanStream.DisposeAsync();
            }

            _logger.LogError(exception, "ClamAV could not be reached while scanning '{FileName}'.", context.FileName);

            throw new AntivirusScanningException($"The ClamAV antivirus scanner could not be reached while scanning '{context.FileName}'.", exception);
        }
        catch (IOException exception)
        {
            if (scanStream != stream)
            {
                await scanStream.DisposeAsync();
            }

            _logger.LogError(exception, "ClamAV failed while scanning '{FileName}'.", context.FileName);

            throw new AntivirusScanningException($"The ClamAV antivirus scanner failed while scanning '{context.FileName}'.", exception);
        }
        catch
        {
            if (scanStream != stream)
            {
                await scanStream.DisposeAsync();
            }

            throw;
        }
    }

    public Task CreatedAsync(IFileStoreEntry fileInfo, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    private static async Task<Stream> CreateSeekableStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var tempStream = new FileStream(
            tempFilePath,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.None,
            81920,
            FileOptions.Asynchronous | FileOptions.DeleteOnClose);

        try
        {
            await stream.CopyToAsync(tempStream, cancellationToken);
            tempStream.Position = 0;

            return tempStream;
        }
        catch
        {
            await tempStream.DisposeAsync();
            throw;
        }
    }

    private static FileCreatingResult TryCreateFailureResult(FileCreatingContext context, Stream stream, string response)
    {
        if (string.Equals(response, "stream: OK", StringComparison.Ordinal))
        {
            return null;
        }

        if (response.EndsWith(" FOUND", StringComparison.Ordinal))
        {
            var signature = response;
            var separatorIndex = signature.IndexOf(": ", StringComparison.Ordinal);

            if (separatorIndex >= 0)
            {
                signature = signature[(separatorIndex + 2)..];
            }

            signature = signature[..^" FOUND".Length];

            stream.Position = 0;

            return FileCreatingResult.Failed(stream, new ResultError
            {
                Message = new LocalizedString(nameof(ClamAvFileEventHandler), $"The uploaded file '{context.FileName}' was rejected because ClamAV detected '{signature}'."),
            });
        }

        throw new AntivirusScanningException(
            $"The ClamAV antivirus scanner returned an unexpected response while scanning '{context.FileName}': {response}");
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            throw new AntivirusScanningException("The ClamAV antivirus scanner is enabled but the host setting is missing.");
        }

        if (_options.Port is < 1 or > 65535)
        {
            throw new AntivirusScanningException("The ClamAV antivirus scanner is enabled but the port setting is invalid.");
        }

        if (_options.ConnectTimeoutSeconds <= 0)
        {
            throw new AntivirusScanningException("The ClamAV antivirus scanner is enabled but the connection timeout must be greater than zero.");
        }

        if (_options.TransferTimeoutSeconds <= 0)
        {
            throw new AntivirusScanningException("The ClamAV antivirus scanner is enabled but the transfer timeout must be greater than zero.");
        }
    }
}

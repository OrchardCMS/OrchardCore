using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;

namespace OrchardCore.Antivirus.ClamAV.Services;

public sealed class ClamAVAntivirusScanner : IAntivirusScanner
{
    private const int BufferSize = 81920;
    private static readonly byte[] _scanCommand = "nINSTREAM\n"u8.ToArray();
    private readonly ClamAvOptions _options;
    private readonly ILogger _logger;

    public ClamAVAntivirusScanner(
        IOptions<ClamAvOptions> options,
        ILogger<ClamAVAntivirusScanner> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AntivirusResult> ScanAsync(AntivirusScanContext context, Stream stream)
    {
        ValidateOptions();

        try
        {
            using var client = new TcpClient();
            using var connectCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(_options.ConnectTimeoutSeconds));

            await client.ConnectAsync(_options.Host, _options.Port, connectCancellationTokenSource.Token);

            client.SendTimeout = (int)TimeSpan.FromSeconds(_options.TransferTimeoutSeconds).TotalMilliseconds;
            client.ReceiveTimeout = (int)TimeSpan.FromSeconds(_options.TransferTimeoutSeconds).TotalMilliseconds;

            await using var networkStream = client.GetStream();
            using var transferCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TransferTimeoutSeconds));

            await networkStream.WriteAsync(_scanCommand, transferCancellationTokenSource.Token);

            var buffer = new byte[BufferSize];

            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), transferCancellationTokenSource.Token)) > 0)
            {
                var prefix = new byte[sizeof(int)];
                BinaryPrimitives.WriteInt32BigEndian(prefix, bytesRead);

                await networkStream.WriteAsync(prefix, transferCancellationTokenSource.Token);
                await networkStream.WriteAsync(buffer.AsMemory(0, bytesRead), transferCancellationTokenSource.Token);
            }

            await networkStream.WriteAsync(new byte[sizeof(int)], transferCancellationTokenSource.Token);
            await networkStream.FlushAsync(transferCancellationTokenSource.Token);

            var response = await ReadResponseAsync(networkStream, transferCancellationTokenSource.Token);

            return CreateResult(context, response);
        }
        catch (OperationCanceledException exception)
        {
            _logger.LogError(exception, "ClamAV timed out while scanning '{FileName}'.", context.FileName);

            throw new AntivirusScanningException($"The ClamAV antivirus scanner timed out while scanning '{context.FileName}'.", exception);
        }
        catch (SocketException exception)
        {
            _logger.LogError(exception, "ClamAV could not be reached while scanning '{FileName}'.", context.FileName);

            throw new AntivirusScanningException($"The ClamAV antivirus scanner could not be reached while scanning '{context.FileName}'.", exception);
        }
        catch (IOException exception)
        {
            _logger.LogError(exception, "ClamAV failed while scanning '{FileName}'.", context.FileName);

            throw new AntivirusScanningException($"The ClamAV antivirus scanner failed while scanning '{context.FileName}'.", exception);
        }
    }

    private static AntivirusResult CreateResult(AntivirusScanContext context, string response)
    {
        if (string.Equals(response, "stream: OK", StringComparison.Ordinal))
        {
            return AntivirusResult.Clean;
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

            return AntivirusResult.Unsafe(
                $"The uploaded file '{context.FileName}' was rejected because ClamAV detected '{signature}'.",
                signature);
        }

        throw new AntivirusScanningException(
            $"The ClamAV antivirus scanner returned an unexpected response while scanning '{context.FileName}': {response}");
    }

    private static async Task<string> ReadResponseAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        using var responseStream = new MemoryStream();
        var buffer = new byte[1];

        while (true)
        {
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, 1), cancellationToken);

            if (bytesRead == 0)
            {
                break;
            }

            if (buffer[0] == '\n')
            {
                break;
            }

            responseStream.WriteByte(buffer[0]);
        }

        return Encoding.ASCII.GetString(responseStream.ToArray());
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

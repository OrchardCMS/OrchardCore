using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Antivirus.ClamAV;

internal sealed class ClamAvConnection : IDisposable
{
    private const int BufferSize = 81920;
    private static readonly byte[] _scanCommand = "nINSTREAM\n"u8.ToArray();

    private readonly ClamAvOptions _options;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private TcpClient _client;
    private NetworkStream _networkStream;

    public ClamAvConnection(ClamAvOptions options, ILogger<ClamAvConnection> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<string> ScanAsync(Stream stream, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            try
            {
                await EnsureConnectedAsync(cancellationToken);

                using var transferCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                transferCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(_options.TransferTimeoutSeconds));

                await _networkStream.WriteAsync(_scanCommand, transferCancellationTokenSource.Token);

                var buffer = new byte[BufferSize];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), transferCancellationTokenSource.Token)) > 0)
                {
                    var prefix = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(bytesRead));

                    await _networkStream.WriteAsync(prefix, transferCancellationTokenSource.Token);
                    await _networkStream.WriteAsync(buffer.AsMemory(0, bytesRead), transferCancellationTokenSource.Token);
                }

                await _networkStream.WriteAsync(new byte[sizeof(int)], transferCancellationTokenSource.Token);
                await _networkStream.FlushAsync(transferCancellationTokenSource.Token);

                return await ReadResponseAsync(_networkStream, transferCancellationTokenSource.Token);
            }
            catch (Exception exception) when (
                exception is IOException or
                SocketException or
                OperationCanceledException or
                ObjectDisposedException)
            {
                ResetConnection();
                throw;
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        ResetConnection();
        _lock.Dispose();
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_client?.Connected == true && _networkStream is not null)
        {
            return;
        }

        ResetConnection();

        var client = new TcpClient();
        using var connectCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        connectCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(_options.ConnectTimeoutSeconds));

        await client.ConnectAsync(_options.Host, _options.Port, connectCancellationTokenSource.Token);

        client.SendTimeout = (int)TimeSpan.FromSeconds(_options.TransferTimeoutSeconds).TotalMilliseconds;
        client.ReceiveTimeout = (int)TimeSpan.FromSeconds(_options.TransferTimeoutSeconds).TotalMilliseconds;

        _client = client;
        _networkStream = client.GetStream();

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Created a shared ClamAV TCP connection for '{Host}:{Port}'.", _options.Host, _options.Port);
        }
    }

    private void ResetConnection()
    {
        _networkStream?.Dispose();
        _networkStream = null;
        _client?.Dispose();
        _client = null;
    }

    private static async Task<string> ReadResponseAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        using var responseStream = new MemoryStream();
        var buffer = new byte[1];

        while (true)
        {
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, 1), cancellationToken);

            if (bytesRead == 0 || buffer[0] == '\n')
            {
                break;
            }

            responseStream.WriteByte(buffer[0]);
        }

        return Encoding.ASCII.GetString(responseStream.ToArray());
    }
}

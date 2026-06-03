using System.Buffers.Binary;
using System.Net.Sockets;
using OrchardCore.Antivirus.ClamAV;
using OrchardCore.Antivirus.ClamAV.Services;
using OrchardCore.FileStorage;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class ClamAVAntivirusScannerTests
{
    [Fact]
    public async Task ScanAsync_ReturnsCleanResult_WhenClamAvReturnsOk()
    {
        await using var server = await FakeClamAvServer.StartAsync("stream: OK\n");
        var scanner = CreateScanner(server.Port);

        var result = await scanner.ScanAsync(
            new AntivirusScanContext("folder/test.txt"),
            new MemoryStream("hello world"u8.ToArray()));

        var request = await server.Completion;

        Assert.True(result.IsClean);
        Assert.Equal("nINSTREAM\n", request.Command);
        Assert.Equal("hello world"u8.ToArray(), request.Payload);
    }

    [Fact]
    public async Task ScanAsync_ReturnsUnsafeResult_WhenClamAvFindsMalware()
    {
        await using var server = await FakeClamAvServer.StartAsync("stream: Eicar-Test-Signature FOUND\n");
        var scanner = CreateScanner(server.Port);

        var result = await scanner.ScanAsync(
            new AntivirusScanContext("folder/test.txt"),
            new MemoryStream("hello world"u8.ToArray()));

        Assert.False(result.IsClean);
        Assert.Equal("Eicar-Test-Signature", result.ThreatName);
        Assert.Equal("The uploaded file 'test.txt' was rejected because ClamAV detected 'Eicar-Test-Signature'.", result.Message);
    }

    [Fact]
    public async Task ScanAsync_Throws_WhenClamAvReturnsError()
    {
        await using var server = await FakeClamAvServer.StartAsync("INSTREAM size limit exceeded. ERROR\n");
        var scanner = CreateScanner(server.Port);

        var exception = await Assert.ThrowsAsync<AntivirusScanningException>(() =>
            scanner.ScanAsync(
                new AntivirusScanContext("folder/test.txt"),
                new MemoryStream("hello world"u8.ToArray())));

        Assert.Equal("The ClamAV antivirus scanner returned an unexpected response while scanning 'test.txt': INSTREAM size limit exceeded. ERROR", exception.Message);
    }

    [Fact]
    public async Task ScanAsync_Throws_WhenHostIsMissing()
    {
        var scanner = new ClamAVAntivirusScanner(
            Options.Create(new ClamAvOptions
            {
                Host = "",
            }),
            Mock.Of<ILogger<ClamAVAntivirusScanner>>());

        var exception = await Assert.ThrowsAsync<AntivirusScanningException>(() =>
            scanner.ScanAsync(
                new AntivirusScanContext("folder/test.txt"),
                new MemoryStream("hello world"u8.ToArray())));

        Assert.Equal("The ClamAV antivirus scanner is enabled but the host setting is missing.", exception.Message);
    }

    private static ClamAVAntivirusScanner CreateScanner(int port) =>
        new(
            Options.Create(new ClamAvOptions
            {
                Host = IPAddress.Loopback.ToString(),
                Port = port,
                ConnectTimeoutSeconds = 5,
                TransferTimeoutSeconds = 5,
            }),
            Mock.Of<ILogger<ClamAVAntivirusScanner>>());

    private sealed class FakeClamAvServer : IAsyncDisposable
    {
        private readonly TcpListener _listener;

        private FakeClamAvServer(TcpListener listener, Task<ClamAvRequest> completion)
        {
            _listener = listener;
            Completion = completion;
        }

        public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

        public Task<ClamAvRequest> Completion { get; }

        public static Task<FakeClamAvServer> StartAsync(string response)
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();

            var completion = Task.Run(async () =>
            {
                using var client = await listener.AcceptTcpClientAsync();
                await using var networkStream = client.GetStream();

                var command = await ReadLineAsync(networkStream);
                var payload = await ReadPayloadAsync(networkStream);
                var responseBytes = Encoding.ASCII.GetBytes(response);

                await networkStream.WriteAsync(responseBytes);
                await networkStream.FlushAsync();

                return new ClamAvRequest(command, payload);
            });

            return Task.FromResult(new FakeClamAvServer(listener, completion));
        }

        public ValueTask DisposeAsync()
        {
            _listener.Stop();

            return ValueTask.CompletedTask;
        }

        private static async Task<string> ReadLineAsync(NetworkStream stream)
        {
            using var commandStream = new MemoryStream();
            var buffer = new byte[1];

            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, 1));

                if (bytesRead == 0)
                {
                    break;
                }

                commandStream.WriteByte(buffer[0]);

                if (buffer[0] == '\n')
                {
                    break;
                }
            }

            return Encoding.ASCII.GetString(commandStream.ToArray());
        }

        private static async Task<byte[]> ReadPayloadAsync(Stream stream)
        {
            using var payloadStream = new MemoryStream();
            var lengthBuffer = new byte[sizeof(int)];

            while (true)
            {
                await ReadExactlyAsync(stream, lengthBuffer);

                var chunkLength = BinaryPrimitives.ReadInt32BigEndian(lengthBuffer);

                if (chunkLength == 0)
                {
                    break;
                }

                var chunk = new byte[chunkLength];
                await ReadExactlyAsync(stream, chunk);
                await payloadStream.WriteAsync(chunk);
            }

            return payloadStream.ToArray();
        }

        private static async Task ReadExactlyAsync(Stream stream, byte[] buffer)
        {
            var offset = 0;

            while (offset < buffer.Length)
            {
                var bytesRead = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset));

                if (bytesRead == 0)
                {
                    throw new EndOfStreamException();
                }

                offset += bytesRead;
            }
        }
    }

    private sealed record ClamAvRequest(string Command, byte[] Payload);
}

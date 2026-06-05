using System.Buffers.Binary;
using System.Net.Sockets;
using OrchardCore.Antivirus.ClamAV;
using OrchardCore.FileStorage;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class ClamAvFileEventHandlerTests
{
    [Fact]
    public async Task CreatingAsync_ReturnsSeekableStream_WhenClamAvReturnsOk()
    {
        await using var server = await FakeClamAvServer.StartAsync("stream: OK\n");
        using var factory = CreateFactory();
        var handler = CreateHandler(server.Port, factory);

        var inputStream = new MemoryStream("hello world"u8.ToArray());
        var result = await handler.CreatingAsync(new FileCreatingContext("folder/test.txt"), inputStream, TestContext.Current.CancellationToken);
        var request = (await server.Completion).Single();

        Assert.True(result.Succeeded);
        Assert.Same(inputStream, result.Stream);
        Assert.Equal("nINSTREAM\n", request.Command);
        Assert.Equal("hello world"u8.ToArray(), request.Payload);
    }

    [Fact]
    public async Task CreatingAsync_BuffersNonSeekableStreams()
    {
        await using var server = await FakeClamAvServer.StartAsync("stream: OK\n");
        using var factory = CreateFactory();
        var handler = CreateHandler(server.Port, factory);
        await using var baseStream = new MemoryStream("hello world"u8.ToArray());
        await using var inputStream = new NonSeekableReadStream(baseStream);

        var result = await handler.CreatingAsync(new FileCreatingContext("folder/test.txt"), inputStream);

        Assert.True(result.Succeeded);
        Assert.NotSame(inputStream, result.Stream);
        Assert.True(result.Stream.CanSeek);

        using var copy = new MemoryStream();
        await result.Stream.CopyToAsync(copy);
        Assert.Equal("hello world"u8.ToArray(), copy.ToArray());
    }

    [Fact]
    public async Task CreatingAsync_ReturnsFailedResult_WhenClamAvFindsMalware()
    {
        await using var server = await FakeClamAvServer.StartAsync("stream: Eicar-Test-Signature FOUND\n");
        using var factory = CreateFactory();
        var handler = CreateHandler(server.Port, factory);

        var result = await handler.CreatingAsync(new FileCreatingContext("folder/test.txt"), new MemoryStream("hello world"u8.ToArray()));

        Assert.False(result.Succeeded);
        Assert.Equal("The uploaded file 'test.txt' was rejected because ClamAV detected 'Eicar-Test-Signature'.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreatingAsync_Throws_WhenClamAvReturnsError()
    {
        await using var server = await FakeClamAvServer.StartAsync("INSTREAM size limit exceeded. ERROR\n");
        using var factory = CreateFactory();
        var handler = CreateHandler(server.Port, factory);

        var exception = await Assert.ThrowsAsync<AntivirusScanningException>(() =>
            handler.CreatingAsync(new FileCreatingContext("folder/test.txt"), new MemoryStream("hello world"u8.ToArray())));

        Assert.Equal("The ClamAV antivirus scanner returned an unexpected response while scanning 'test.txt': INSTREAM size limit exceeded. ERROR", exception.Message);
    }

    [Fact]
    public async Task CreatingAsync_Throws_WhenHostIsMissing()
    {
        using var factory = CreateFactory();
        var handler = new ClamAvFileEventHandler(
            Options.Create(new ClamAvOptions
            {
                Host = "",
            }),
            factory,
            NullLogger<ClamAvFileEventHandler>.Instance);

        var exception = await Assert.ThrowsAsync<AntivirusScanningException>(() =>
            handler.CreatingAsync(new FileCreatingContext("folder/test.txt"), new MemoryStream("hello world"u8.ToArray())));

        Assert.Equal("The ClamAV antivirus scanner is enabled but the host setting is missing.", exception.Message);
    }

    [Fact]
    public async Task CreatingAsync_ReusesTheSameTcpConnection()
    {
        await using var server = await FakeClamAvServer.StartAsync("stream: OK\n", "stream: OK\n");
        using var factory = CreateFactory();
        var handler = CreateHandler(server.Port, factory);

        var firstResult = await handler.CreatingAsync(new FileCreatingContext("folder/first.txt"), new MemoryStream("first"u8.ToArray()));
        var secondResult = await handler.CreatingAsync(new FileCreatingContext("folder/second.txt"), new MemoryStream("second"u8.ToArray()));
        var requests = await server.Completion;

        Assert.True(firstResult.Succeeded);
        Assert.True(secondResult.Succeeded);
        Assert.Equal(1, server.ConnectionCount);
        Assert.Equal("first"u8.ToArray(), requests[0].Payload);
        Assert.Equal("second"u8.ToArray(), requests[1].Payload);
    }

    private static ClamAvFileEventHandler CreateHandler(int port, ClamAvConnectionFactory factory)
        => new(
            Options.Create(new ClamAvOptions
            {
                Host = IPAddress.Loopback.ToString(),
                Port = port,
                ConnectTimeoutSeconds = 5,
                TransferTimeoutSeconds = 5,
            }),
            factory,
            NullLogger<ClamAvFileEventHandler>.Instance);

    private static ClamAvConnectionFactory CreateFactory()
        => new(Mock.Of<IHostApplicationLifetime>(l => l.ApplicationStopped == CancellationToken.None), NullLoggerFactory.Instance);

    private sealed class FakeClamAvServer : IAsyncDisposable
    {
        private readonly TcpListener _listener;

        private FakeClamAvServer(TcpListener listener, Task<ClamAvRequest[]> completion, Func<int> connectionCount)
        {
            _listener = listener;
            Completion = completion;
            _connectionCount = connectionCount;
        }

        private readonly Func<int> _connectionCount;

        public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

        public int ConnectionCount => _connectionCount();

        public Task<ClamAvRequest[]> Completion { get; }

        public static Task<FakeClamAvServer> StartAsync(params string[] responses)
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();

            var connectionCount = 0;
            var completion = Task.Run(async () =>
            {
                var requests = new List<ClamAvRequest>();

                using var client = await listener.AcceptTcpClientAsync();
                Interlocked.Increment(ref connectionCount);

                await using var networkStream = client.GetStream();

                foreach (var response in responses)
                {
                    var command = await ReadLineAsync(networkStream);
                    var payload = await ReadPayloadAsync(networkStream);
                    var responseBytes = Encoding.ASCII.GetBytes(response);

                    requests.Add(new ClamAvRequest(command, payload));

                    await networkStream.WriteAsync(responseBytes);
                    await networkStream.FlushAsync();
                }

                return requests.ToArray();
            });

            return Task.FromResult(new FakeClamAvServer(listener, completion, () => connectionCount));
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

    private sealed class NonSeekableReadStream : Stream
    {
        private readonly Stream _innerStream;

        public NonSeekableReadStream(Stream innerStream)
        {
            _innerStream = innerStream;
        }

        public override bool CanRead => _innerStream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() => _innerStream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => _innerStream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override async ValueTask DisposeAsync()
        {
            await _innerStream.DisposeAsync();
            await base.DisposeAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _innerStream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

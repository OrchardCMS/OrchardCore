using System.Net;

namespace OrchardCore.Cli.Tests;

public class FileResponseTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Download_RequiresNewDestination_PreservesBytes_AndCleansFailedRequests(bool failure)
    {
        var token = TestContext.Current.CancellationToken;
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(Download_RequiresNewDestination_PreservesBytes_AndCleansFailedRequests)));
        await new ContextStore(paths).SaveAsync(new CliConfiguration
        {
            CurrentContext = "site", Contexts = [new TenantContextRecord { Name = "site", TenantUrl = "https://cms.example/" }],
        }, token);
        await new CacheService(paths).WriteAsync("https://cms.example/", CacheKind.OpenApi, new CachedContentRecord
        {
            Content = """
            {"paths":{"/api/download":{"get":{"operationId":"Download","x-oc-cli":{"commandGroup":["files"],"verb":"download","fileResponse":true}}}}}
            """,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
        }, token);
        var handler = new DownloadHandler(failure);
        using var client = new HttpClient(handler);
        string[] missing = ["files", "download", "--output", "none"];
        var app = await CliApplication.CreateAsync(missing, paths, client, token, new FileCredentialStore(paths));
        Assert.NotEqual(0, await app.InvokeAsync(missing));
        Assert.Equal(0, handler.Requests);
        var destination = Path.Combine(paths.RootDirectory, "package.zip");
        string[] args = ["files", "download", "--output-file", destination, "--output", "none"];
        var result = await app.InvokeAsync(args);
        Assert.Equal(1, handler.Requests);
        if (failure)
        {
            Assert.NotEqual(0, result);
            Assert.False(File.Exists(destination));
        }
        else
        {
            Assert.Equal(0, result);
            Assert.Equal(DownloadHandler.Bytes, await File.ReadAllBytesAsync(destination, token));
            Assert.NotEqual(0, await app.InvokeAsync(args));
            Assert.Equal(1, handler.Requests);
            Assert.Equal(DownloadHandler.Bytes, await File.ReadAllBytesAsync(destination, token));
        }
    }

    [Fact]
    public async Task InterruptedDownload_RemovesPartiallyWrittenDestination()
    {
        var path = Path.Combine(TestPaths.CreateScratchDirectory(nameof(InterruptedDownload_RemovesPartiallyWrittenDestination)), "partial.zip");
        await using (var file = SecretOutputFile.Create(new FileInfo(path)))
        {
            using var response = new InterruptedStream();
            await Assert.ThrowsAsync<IOException>(() => file.WriteStreamAsync(response, TestContext.Current.CancellationToken));
            Assert.True(File.Exists(path));
        }
        Assert.False(File.Exists(path));
    }

    private sealed class InterruptedStream : MemoryStream
    {
        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            await destination.WriteAsync(new byte[] { 0x50, 0x4b }, cancellationToken);
            throw new IOException("Synthetic interrupted transfer");
        }
    }

    private sealed class DownloadHandler(bool failure) : HttpMessageHandler
    {
        public static readonly byte[] Bytes = [0x50, 0x4b, 0, 0xff, 0xfe, 0x80, 0x7b];
        public int Requests { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal("/api/download", request.RequestUri!.AbsolutePath);
            Requests++;
            return Task.FromResult(new HttpResponseMessage(failure ? HttpStatusCode.Forbidden : HttpStatusCode.OK)
            {
                Content = failure ? new StringContent("denied") : new ByteArrayContent(Bytes),
            });
        }
    }
}

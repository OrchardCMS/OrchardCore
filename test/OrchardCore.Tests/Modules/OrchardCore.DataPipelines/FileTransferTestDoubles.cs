using OrchardCore.DataPipelines.Steps;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

/// <summary>
/// Assertions shared by the tests of the destination steps.
/// </summary>
internal static class DestinationStepAssert
{
    public static List<string> Errors(DataPipelineDescribeContext description)
        => description.Issues.Where(issue => issue.Severity == DataPipelineIssueSeverity.Error).Select(issue => issue.Message).ToList();

    public static void AssertNoSecrets(StepTestHost host, params string[] secrets)
    {
        var texts = host.Run.Deliveries.SelectMany(delivery => new[] { delivery.Description, delivery.Url })
            .Concat(host.Log.Select(entry => entry.Message))
            .Where(text => text is not null)
            .ToList();

        Assert.NotEmpty(texts);

        foreach (var secret in secrets)
        {
            Assert.DoesNotContain(texts, text => text.Contains(secret, StringComparison.Ordinal));
        }
    }
}

/// <summary>
/// A file server in memory, that the fake clients of the FTP and SFTP steps upload files to.
/// </summary>
internal sealed class FakeRemoteServer
{
    public Dictionary<string, byte[]> Files { get; } = new(StringComparer.Ordinal);

    public HashSet<string> Folders { get; } = new(StringComparer.Ordinal);

    public Exception ConnectError { get; set; }

    public string Fingerprint { get; set; } = "ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6Og";

    public bool Authenticated { get; set; }

    public bool Disposed { get; set; }

    public string Text(string path) => Encoding.UTF8.GetString(Files[path]);
}

/// <summary>
/// A client that uploads files to a <see cref="FakeRemoteServer"/>.
/// </summary>
internal sealed class FakeTransferClient : IDataPipelineFileTransferClient
{
    private readonly FakeRemoteServer _server;
    private readonly Func<string, bool> _validateHostKey;
    private bool _connected;

    public FakeTransferClient(FakeRemoteServer server, Func<string, bool> validateHostKey = null)
    {
        _server = server;
        _validateHostKey = validateHostKey;
    }

    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (_validateHostKey is not null && !_validateHostKey(_server.Fingerprint))
        {
            throw new InvalidOperationException("Key exchange negotiation failed.");
        }

        if (_server.ConnectError is not null)
        {
            throw _server.ConnectError;
        }

        _connected = true;
        _server.Authenticated = true;

        return Task.CompletedTask;
    }

    public Task EnsureFolderAsync(string folder, CancellationToken cancellationToken)
    {
        Assert.True(_connected);
        _server.Folders.Add(folder);

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string remotePath, CancellationToken cancellationToken)
    {
        Assert.True(_connected);

        return Task.FromResult(_server.Files.ContainsKey(remotePath));
    }

    public async Task UploadAsync(Stream content, string remotePath, bool overwrite, CancellationToken cancellationToken)
    {
        Assert.True(_connected);

        if (!overwrite && _server.Files.ContainsKey(remotePath))
        {
            throw new IOException("The file exists.");
        }

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        _server.Files[remotePath] = buffer.ToArray();
    }

    public ValueTask DisposeAsync()
    {
        _server.Disposed = true;

        return ValueTask.CompletedTask;
    }
}

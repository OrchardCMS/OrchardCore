using System.Security.Cryptography;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Sftp;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.Infrastructure;
using OrchardCore.Tests.Modules.OrchardCore.DataSources;
using static OrchardCore.Tests.Modules.OrchardCore.DataPipelines.DestinationStepAssert;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class DataPipelinesSftpTests
{
    private const string Password = "s3cr3t-P@ss";
    private const string PrivateKey = "-----BEGIN OPENSSH PRIVATE KEY-----\nsecret-key-material\n-----END OPENSSH PRIVATE KEY-----";
    private const string Passphrase = "key-passphrase";

    private readonly DataPipelineSecrets _secrets = new(new EphemeralDataProtectionProvider());

    [Fact]
    public async Task UploadToSftp_MissingSettings_ReportsErrors()
    {
        // Arrange
        using var host = new StepTestHost(SftpStep(new FakeSftpClientFactory()), new UploadToSftpStepSettings { HostKeyFingerprint = "not a fingerprint!" });

        // Act
        var description = await host.DescribeAsync();

        // Assert
        var errors = Errors(description);
        Assert.Contains(errors, message => message.Contains("host", StringComparison.OrdinalIgnoreCase) && !message.Contains("fingerprint", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, message => message.Contains("user", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, message => message.Contains("password or a private key", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, message => message.Contains("fingerprint", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task UploadToSftp_MatchingFingerprint_UploadsWithPrivateKey()
    {
        // Arrange
        var factory = new FakeSftpClientFactory();
        var fingerprint = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("host key")));
        factory.Server.Fingerprint = fingerprint.TrimEnd('=');
        var settings = new UploadToSftpStepSettings
        {
            Host = "sftp.example.com",
            Username = "deploy",
            ProtectedPrivateKey = _secrets.Protect(PrivateKey),
            ProtectedPassphrase = _secrets.Protect(Passphrase),
            HostKeyFingerprint = "SHA256:" + fingerprint,
            RemoteFolder = "incoming",
        };
        using var host = new StepTestHost(SftpStep(factory), settings);
        host.WithFiles(host.CreateFile("a.csv", "content"));

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(22, factory.Connection.Port);
        Assert.Equal(PrivateKey, factory.Connection.PrivateKey);
        Assert.Equal(Passphrase, factory.Connection.Passphrase);
        Assert.Null(factory.Connection.Password);
        Assert.Equal("content", factory.Server.Text("incoming/a.csv"));
        Assert.Equal("Uploaded 'a.csv' to sftp://sftp.example.com:22/incoming/a.csv", host.Run.Deliveries.Single().Description);
        Assert.DoesNotContain(host.Log, entry => entry.Level == LogLevel.Warning);
        AssertNoSecrets(host, PrivateKey, Passphrase, "secret-key-material");
    }

    [Fact]
    public async Task UploadToSftp_FingerprintMismatch_RefusesToConnect()
    {
        // Arrange
        var factory = new FakeSftpClientFactory();
        factory.Server.Fingerprint = "ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6Og";
        var settings = new UploadToSftpStepSettings
        {
            Host = "sftp.example.com",
            Username = "deploy",
            ProtectedPassword = _secrets.Protect(Password),
            HostKeyFingerprint = "SHA256:" + Convert.ToBase64String(new byte[32]),
        };
        using var host = new StepTestHost(SftpStep(factory), settings);
        host.WithFiles(host.CreateFile("a.csv", "content"));

        // Act
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => host.ExecuteAsync(TestContext.Current.CancellationToken));

        // Assert
        Assert.Contains("doesn't match", exception.Message, StringComparison.Ordinal);
        Assert.Contains("ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6Og", exception.Message, StringComparison.Ordinal);
        Assert.False(factory.Server.Authenticated);
        Assert.Empty(factory.Server.Files);
        Assert.Empty(host.Run.Deliveries);
        Assert.DoesNotContain(Password, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UploadToSftp_NoFingerprint_WarnsHostIsNotVerified()
    {
        // Arrange
        var factory = new FakeSftpClientFactory();
        factory.Server.Fingerprint = "ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6Og";
        var settings = new UploadToSftpStepSettings
        {
            Host = "sftp.example.com",
            Username = "deploy",
            ProtectedPassword = _secrets.Protect(Password),
            Overwrite = false,
        };
        using var host = new StepTestHost(SftpStep(factory), settings);
        host.WithFiles(host.CreateFile("a.csv", "content"));

        // Act
        var description = await host.DescribeAsync();
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains(description.Issues, issue => issue.Severity == DataPipelineIssueSeverity.Warning);
        var warning = Assert.Single(host.Log, entry => entry.Level == LogLevel.Warning);
        Assert.Contains("SHA256:ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6Og", warning.Message, StringComparison.Ordinal);
        Assert.Equal(Password, factory.Connection.Password);
        Assert.Equal("content", factory.Server.Text("a.csv"));
        AssertNoSecrets(host, Password);
    }

    [Fact]
    public async Task UploadToSftp_ExistingFileWithoutOverwrite_Fails()
    {
        // Arrange
        var factory = new FakeSftpClientFactory();
        factory.Server.Files["/in/a.csv"] = [1];
        var settings = new UploadToSftpStepSettings
        {
            Host = "sftp.example.com",
            Username = "deploy",
            ProtectedPassword = _secrets.Protect(Password),
            RemoteFolder = "/in",
            Overwrite = false,
        };
        using var host = new StepTestHost(SftpStep(factory), settings);
        host.WithFiles(host.CreateFile("a.csv", "content"));

        // Act
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => host.ExecuteAsync(TestContext.Current.CancellationToken));

        // Assert
        Assert.Contains("already exists", exception.Message, StringComparison.Ordinal);
        Assert.Equal([1], factory.Server.Files["/in/a.csv"]);
    }

    [Theory]
    [InlineData("SHA256:ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6Og", true)]
    [InlineData("sha256:ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6Og=", true)]
    [InlineData(" ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6Og ", true)]
    [InlineData("SHA256:ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6OG", false)]
    [InlineData("SHA256:AAAA", false)]
    public void HostKeyFingerprint_Matches_ComparesNormalizedSha256(string expected, bool matches)
    {
        Assert.Equal(matches, DataPipelineHostKeyFingerprint.Matches(expected, "ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6Og"));
    }

    [Theory]
    [InlineData("SHA256:ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6Og", true)]
    [InlineData("MD5:16:27:ac:a5:76:28:2d:36:63:1b:56:4d:eb:df:a6:48", false)]
    [InlineData("SHA256:short", false)]
    [InlineData("", false)]
    public void HostKeyFingerprint_IsValid_AcceptsSha256Only(string fingerprint, bool valid)
    {
        Assert.Equal(valid, DataPipelineHostKeyFingerprint.IsValid(fingerprint));
    }

    private UploadToSftpStep SftpStep(FakeSftpClientFactory factory)
        => new(factory, _secrets, new PassThroughStringLocalizer<UploadToSftpStep>());

    private sealed class FakeSftpClientFactory : IDataPipelineSftpClientFactory
    {
        public FakeRemoteServer Server { get; } = new();

        public DataPipelineSftpConnection Connection { get; private set; }

        public IDataPipelineFileTransferClient CreateClient(DataPipelineSftpConnection connection)
        {
            Connection = connection;

            return new FakeTransferClient(Server, connection.ValidateHostKey);
        }
    }
}

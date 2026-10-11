using OrchardCore.DataPipelines.Ftp;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.Infrastructure;
using OrchardCore.Tests.Modules.OrchardCore.DataSources;
using static OrchardCore.Tests.Modules.OrchardCore.DataPipelines.DestinationStepAssert;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class DataPipelinesFtpTests
{
    private const string Password = "s3cr3t-P@ss";

    private readonly DataPipelineSecrets _secrets = new(new EphemeralDataProtectionProvider());

    [Fact]
    public async Task UploadToFtp_MissingHostAndUsername_ReportsErrors()
    {
        // Arrange
        using var host = new StepTestHost(FtpStep(new FakeFtpClientFactory()), new UploadToFtpStepSettings());

        // Act
        var description = await host.DescribeAsync();

        // Assert
        var errors = Errors(description);
        Assert.Contains(errors, message => message.Contains("host", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, message => message.Contains("user", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task UploadToFtp_PasswordProtectedOnAnotherSite_ReportsError()
    {
        // Arrange
        var settings = new UploadToFtpStepSettings { Host = "ftp.example.com", Username = "user", ProtectedPassword = "not-a-protected-value" };
        using var host = new StepTestHost(FtpStep(new FakeFtpClientFactory()), settings);

        // Act
        var description = await host.DescribeAsync();

        // Assert
        Assert.Contains(Errors(description), message => message.Contains("password", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task UploadToFtp_Files_UploadsToRenderedFolder()
    {
        // Arrange
        var factory = new FakeFtpClientFactory();
        var settings = new UploadToFtpStepSettings
        {
            Host = "ftp.example.com",
            Port = 2121,
            Username = "user",
            ProtectedPassword = _secrets.Protect(Password),
            RemoteFolder = "/exports/{Date:yyyy}/",
            TimeoutSeconds = 12,
        };
        using var host = new StepTestHost(FtpStep(factory), settings);
        host.WithFiles(host.CreateFile("a.csv", "first"), host.CreateFile("b.csv", "second"));

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        var connection = factory.Connection;
        Assert.Equal("ftp.example.com", connection.Host);
        Assert.Equal(2121, connection.Port);
        Assert.Equal("user", connection.Username);
        Assert.Equal(Password, connection.Password);
        Assert.Equal(DataPipelineFtpEncryption.ExplicitTls, connection.Encryption);
        Assert.True(connection.ValidateCertificate);
        Assert.Equal(TimeSpan.FromSeconds(12), connection.Timeout);

        Assert.Contains("/exports/2026", factory.Server.Folders);
        Assert.Equal("first", factory.Server.Text("/exports/2026/a.csv"));
        Assert.Equal("second", factory.Server.Text("/exports/2026/b.csv"));
        Assert.True(factory.Server.Disposed);

        Assert.Equal(
            ["Uploaded 'a.csv' to ftps://ftp.example.com:2121/exports/2026/a.csv", "Uploaded 'b.csv' to ftps://ftp.example.com:2121/exports/2026/b.csv"],
            host.Run.Deliveries.Select(delivery => delivery.Description));
        AssertNoSecrets(host, Password);
    }

    [Fact]
    public async Task UploadToFtp_ExistingFileWithoutOverwrite_Fails()
    {
        // Arrange
        var factory = new FakeFtpClientFactory();
        factory.Server.Files["/out/a.csv"] = Encoding.UTF8.GetBytes("old");
        var settings = new UploadToFtpStepSettings { Host = "ftp.example.com", Username = "user", RemoteFolder = "/out", Overwrite = false };
        using var host = new StepTestHost(FtpStep(factory), settings);
        host.WithFiles(host.CreateFile("a.csv", "new"));

        // Act
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => host.ExecuteAsync(TestContext.Current.CancellationToken));

        // Assert
        Assert.Contains("already exists", exception.Message, StringComparison.Ordinal);
        Assert.Equal("old", factory.Server.Text("/out/a.csv"));
        Assert.Empty(host.Run.Deliveries);
    }

    [Fact]
    public async Task UploadToFtp_ExistingFileWithOverwrite_ReplacesIt()
    {
        // Arrange
        var factory = new FakeFtpClientFactory();
        factory.Server.Files["a.csv"] = Encoding.UTF8.GetBytes("old");
        var settings = new UploadToFtpStepSettings { Host = "ftp.example.com", Username = "user" };
        using var host = new StepTestHost(FtpStep(factory), settings);
        host.WithFiles(host.CreateFile("a.csv", "new"));

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("new", factory.Server.Text("a.csv"));
        Assert.Empty(factory.Server.Folders);
    }

    [Fact]
    public async Task UploadToFtp_NoEncryption_WarnsCredentialsTravelInClearText()
    {
        // Arrange
        var factory = new FakeFtpClientFactory();
        var settings = new UploadToFtpStepSettings
        {
            Host = "ftp.example.com",
            Username = "user",
            ProtectedPassword = _secrets.Protect(Password),
            Encryption = DataPipelineFtpEncryption.None,
        };
        using var host = new StepTestHost(FtpStep(factory), settings);
        host.WithFiles(host.CreateFile("a.csv", "content"));

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains(host.Log, entry => entry.Level == LogLevel.Warning && entry.Message.Contains("clear text", StringComparison.Ordinal));
        Assert.StartsWith("Uploaded 'a.csv' to ftp://ftp.example.com:21/", host.Run.Deliveries.Single().Description, StringComparison.Ordinal);
        AssertNoSecrets(host, Password);
    }

    [Fact]
    public async Task UploadToFtp_NoFiles_DoesNotConnect()
    {
        // Arrange
        var factory = new FakeFtpClientFactory();
        var settings = new UploadToFtpStepSettings { Host = "ftp.example.com", Username = "user" };
        using var host = new StepTestHost(FtpStep(factory), settings);
        host.WithFiles();

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(factory.Connection);
        Assert.Empty(host.Run.Deliveries);
    }

    [Fact]
    public async Task UploadToFtp_ServerError_FailsWithoutSecrets()
    {
        // Arrange
        var factory = new FakeFtpClientFactory();
        factory.Server.ConnectError = new IOException("530 Login incorrect.");
        var settings = new UploadToFtpStepSettings { Host = "ftp.example.com", Username = "user", ProtectedPassword = _secrets.Protect(Password) };
        using var host = new StepTestHost(FtpStep(factory), settings);
        host.WithFiles(host.CreateFile("a.csv", "content"));

        // Act
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => host.ExecuteAsync(TestContext.Current.CancellationToken));

        // Assert
        Assert.Contains("530 Login incorrect.", exception.Message, StringComparison.Ordinal);
        Assert.Contains("ftp.example.com", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(Password, exception.Message, StringComparison.Ordinal);
    }

    private UploadToFtpStep FtpStep(FakeFtpClientFactory factory)
        => new(factory, _secrets, new PassThroughStringLocalizer<UploadToFtpStep>());

    private sealed class FakeFtpClientFactory : IDataPipelineFtpClientFactory
    {
        public FakeRemoteServer Server { get; } = new();

        public DataPipelineFtpConnection Connection { get; private set; }

        public IDataPipelineFileTransferClient CreateClient(DataPipelineFtpConnection connection)
        {
            Connection = connection;

            return new FakeTransferClient(Server);
        }
    }
}

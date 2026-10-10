using System.Security.Cryptography;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.Email;
using OrchardCore.Infrastructure;
using OrchardCore.Tests.Modules.OrchardCore.DataSources;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class DestinationStepsTests
{
    private const string Password = "s3cr3t-P@ss";
    private const string PrivateKey = "-----BEGIN OPENSSH PRIVATE KEY-----\nsecret-key-material\n-----END OPENSSH PRIVATE KEY-----";
    private const string Passphrase = "key-passphrase";
    private const string Token = "bearer-token-value";
    private const string ApiKey = "api-key-value";

    private readonly DataPipelineSecrets _secrets = new(new EphemeralDataProtectionProvider());

    // FTP

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

    // SFTP

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

    // Web API

    [Theory]
    [InlineData("")]
    [InlineData("api.example.com/upload")]
    [InlineData("/upload")]
    [InlineData("ftp://api.example.com/upload")]
    [InlineData("https://user:pass@api.example.com/upload")]
    public async Task SendToWebApi_InvalidUrl_ReportsError(string url)
    {
        // Arrange
        using var host = new StepTestHost(WebApiStep(new FakeHttpHandler()), new SendToWebApiStepSettings { Url = url });

        // Act
        var description = await host.DescribeAsync();

        // Assert
        Assert.Contains(Errors(description), message => message.Contains("URL", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("Authorization", "Bearer x")]
    [InlineData("Bad Header", "x")]
    [InlineData("X-Line", "a\r\nInjected: b")]
    public async Task SendToWebApi_InvalidHeader_ReportsError(string name, string value)
    {
        // Arrange
        var settings = new SendToWebApiStepSettings { Url = "https://api.example.com/upload", Headers = [new DataPipelineHttpHeader { Name = name, Value = value }] };
        using var host = new StepTestHost(WebApiStep(new FakeHttpHandler()), settings);

        // Act
        var description = await host.DescribeAsync();

        // Assert
        Assert.Single(Errors(description));
    }

    [Theory]
    [InlineData(DataPipelineHttpAuthentication.Basic)]
    [InlineData(DataPipelineHttpAuthentication.Bearer)]
    [InlineData(DataPipelineHttpAuthentication.ApiKey)]
    public async Task SendToWebApi_AuthenticationWithoutSecret_ReportsError(DataPipelineHttpAuthentication authentication)
    {
        // Arrange
        var settings = new SendToWebApiStepSettings { Url = "https://api.example.com/upload", Authentication = authentication, ApiKeyHeaderName = null };
        using var host = new StepTestHost(WebApiStep(new FakeHttpHandler()), settings);

        // Act
        var description = await host.DescribeAsync();

        // Assert
        Assert.NotEmpty(Errors(description));
    }

    [Fact]
    public async Task SendToWebApi_Multipart_PostsOneRequestPerFile()
    {
        // Arrange
        var handler = new FakeHttpHandler { Status = HttpStatusCode.Created };
        var settings = new SendToWebApiStepSettings
        {
            Url = "https://api.example.com/upload?code=query-secret",
            FormFieldName = "document",
            Headers = [new DataPipelineHttpHeader { Name = "X-Tenant", Value = "acme" }],
        };
        using var host = new StepTestHost(WebApiStep(handler), settings);
        host.WithFiles(host.CreateFile("a.csv", "first", "text/csv"), host.CreateFile("b.csv", "second", "text/csv"));

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, handler.Requests.Count);
        var request = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://api.example.com/upload?code=query-secret", request.Uri.ToString());
        Assert.Equal("acme", request.Headers["X-Tenant"]);
        Assert.False(request.Headers.ContainsKey("Authorization"));
        Assert.StartsWith("multipart/form-data", request.ContentType, StringComparison.Ordinal);
        Assert.Matches("name=\"?document\"?;", request.Body);
        Assert.Matches(@"filename=""?a.csv""?", request.Body);
        Assert.Contains("Content-Type: text/csv", request.Body, StringComparison.Ordinal);
        Assert.Contains("first", request.Body, StringComparison.Ordinal);
        Assert.Contains("second", handler.Requests[1].Body, StringComparison.Ordinal);

        Assert.Equal(
            ["Sent 'a.csv' to https://api.example.com/upload (201)", "Sent 'b.csv' to https://api.example.com/upload (201)"],
            host.Run.Deliveries.Select(delivery => delivery.Description));
        AssertNoSecrets(host, "query-secret");
    }

    [Fact]
    public async Task SendToWebApi_RawPut_SendsFileAsBody()
    {
        // Arrange
        var handler = new FakeHttpHandler();
        var settings = new SendToWebApiStepSettings
        {
            Url = "http://localhost:8080/files",
            Method = DataPipelineHttpMethod.Put,
            BodyMode = DataPipelineHttpBodyMode.Raw,
        };
        using var host = new StepTestHost(WebApiStep(handler), settings);
        host.WithFiles(host.CreateFile("data.json", "{\"a\":1}", "application/json"));

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Put, request.Method);
        Assert.Equal("application/json", request.ContentType);
        Assert.Equal("{\"a\":1}", request.Body);
        Assert.Equal("Sent 'data.json' to http://localhost:8080/files (200)", host.Run.Deliveries.Single().Description);
    }

    [Theory]
    [InlineData(DataPipelineHttpAuthentication.Basic, "Authorization", "Basic dXNlcjpzM2NyM3QtUEBzcw==")]
    [InlineData(DataPipelineHttpAuthentication.Bearer, "Authorization", "Bearer " + Token)]
    [InlineData(DataPipelineHttpAuthentication.ApiKey, "X-Api-Key", ApiKey)]
    public async Task SendToWebApi_Authentication_SendsCredentials(DataPipelineHttpAuthentication authentication, string header, string value)
    {
        // Arrange
        var handler = new FakeHttpHandler();
        var settings = new SendToWebApiStepSettings
        {
            Url = "https://api.example.com/upload",
            Authentication = authentication,
            Username = "user",
            ProtectedPassword = _secrets.Protect(Password),
            ProtectedToken = _secrets.Protect(Token),
            ApiKeyHeaderName = "X-Api-Key",
            ProtectedApiKey = _secrets.Protect(ApiKey),
        };
        using var host = new StepTestHost(WebApiStep(handler), settings);
        host.WithFiles(host.CreateFile("a.csv", "content"));

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(value, Assert.Single(handler.Requests).Headers[header]);
        AssertNoSecrets(host, Password, Token, ApiKey, "dXNlcjpzM2NyM3QtUEBzcw==");
    }

    [Fact]
    public async Task SendToWebApi_NonSuccessStatus_FailsWithStatusAndStartOfBody()
    {
        // Arrange
        var handler = new FakeHttpHandler { Status = HttpStatusCode.BadRequest, ResponseBody = "Invalid file: " + new string('x', 1000) };
        var settings = new SendToWebApiStepSettings
        {
            Url = "https://api.example.com/upload?sig=query-secret",
            Authentication = DataPipelineHttpAuthentication.Bearer,
            ProtectedToken = _secrets.Protect(Token),
        };
        using var host = new StepTestHost(WebApiStep(handler), settings);
        host.WithFiles(host.CreateFile("a.csv", "content"), host.CreateFile("b.csv", "content"));

        // Act
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => host.ExecuteAsync(TestContext.Current.CancellationToken));

        // Assert
        Assert.Contains("400", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Invalid file: xxx", exception.Message, StringComparison.Ordinal);
        Assert.Contains(new string('x', 500 - "Invalid file: ".Length), exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(new string('x', 501 - "Invalid file: ".Length), exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("query-secret", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(Token, exception.Message, StringComparison.Ordinal);
        Assert.Single(handler.Requests);
        Assert.Empty(host.Run.Deliveries);
    }

    // Email

    [Fact]
    public async Task SendByEmail_MissingOrInvalidRecipients_ReportsErrors()
    {
        // Arrange
        using var empty = new StepTestHost(EmailStep(), new SendByEmailStepSettings { To = " ; " });
        using var invalid = new StepTestHost(EmailStep(), new SendByEmailStepSettings { To = "ada@example.com", Cc = "not-an-address", MaxAttachmentSizeMegabytes = 0 });

        // Act
        var emptyDescription = await empty.DescribeAsync();
        var invalidDescription = await invalid.DescribeAsync();

        // Assert
        Assert.Contains(Errors(emptyDescription), message => message.Contains("recipient", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(Errors(invalidDescription), message => message.Contains("not-an-address", StringComparison.Ordinal));
        Assert.Contains(Errors(invalidDescription), message => message.Contains("size", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SendByEmail_Files_SendsOneEmailWithEveryFileAttached()
    {
        // Arrange
        var email = new FakeEmailService();
        var settings = new SendByEmailStepSettings
        {
            To = "ada@example.com; Bob <bob@example.com>",
            Cc = "cy@example.com",
            Bcc = "audit@example.com",
            Subject = "{PipelineName} {Date:yyyyMMdd}",
            Body = "Attached: {FileNames} ({RunId}).",
        };
        using var host = new StepTestHost(EmailStep(), settings, EmailServices(email));
        host.WithFiles(host.CreateFile("a.csv", "first"), host.CreateFile("b.csv", "second"));

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        var message = Assert.Single(email.Messages);
        Assert.Equal("ada@example.com,Bob <bob@example.com>", message.To);
        Assert.Equal("cy@example.com", message.Cc);
        Assert.Equal("audit@example.com", message.Bcc);
        Assert.Equal("Test pipeline 20260315", message.Subject);
        Assert.Equal("Attached: a.csv, b.csv (run).", message.TextBody);
        Assert.Null(message.HtmlBody);
        Assert.Equal(["a.csv", "b.csv"], email.Attachments.Select(attachment => attachment.Name));
        Assert.Equal(["first", "second"], email.Attachments.Select(attachment => attachment.Content));

        var delivery = Assert.Single(host.Run.Deliveries);
        Assert.Contains("ada@example.com", delivery.Description, StringComparison.Ordinal);
        Assert.Contains("'a.csv'", delivery.Description, StringComparison.Ordinal);
        Assert.DoesNotContain("audit@example.com", delivery.Description, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SendByEmail_HtmlBody_EncodesFileNames()
    {
        // Arrange
        var email = new FakeEmailService();
        var settings = new SendByEmailStepSettings { To = "ada@example.com", Body = "<p>{FileNames}</p>", IsHtmlBody = true };
        using var host = new StepTestHost(EmailStep(), settings, EmailServices(email));
        host.WithFiles(host.CreateFile("a&b.csv", "first"));

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        var message = Assert.Single(email.Messages);
        Assert.Equal("<p>a&amp;b.csv</p>", message.HtmlBody);
        Assert.Null(message.TextBody);
    }

    [Fact]
    public async Task SendByEmail_FilesLargerThanLimit_FailsWithoutSending()
    {
        // Arrange
        var email = new FakeEmailService();
        var settings = new SendByEmailStepSettings { To = "ada@example.com", MaxAttachmentSizeMegabytes = 1 };
        using var host = new StepTestHost(EmailStep(), settings, EmailServices(email));
        host.WithFiles(host.CreateFile("a.csv", new string('a', 600 * 1024)), host.CreateFile("b.csv", new string('b', 600 * 1024)));

        // Act
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => host.ExecuteAsync(TestContext.Current.CancellationToken));

        // Assert
        Assert.Contains("1 MB", exception.Message, StringComparison.Ordinal);
        Assert.Empty(email.Messages);
        Assert.Empty(host.Run.Deliveries);
    }

    [Fact]
    public async Task SendByEmail_NoFiles_SkipsByDefault()
    {
        // Arrange
        var email = new FakeEmailService();
        using var host = new StepTestHost(EmailStep(), new SendByEmailStepSettings { To = "ada@example.com" }, EmailServices(email));
        host.WithFiles();

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(email.Messages);
        Assert.Empty(host.Run.Deliveries);
    }

    [Fact]
    public async Task SendByEmail_NoFilesAndSendWhenEmpty_SendsWithoutAttachments()
    {
        // Arrange
        var email = new FakeEmailService();
        using var host = new StepTestHost(EmailStep(), new SendByEmailStepSettings { To = "ada@example.com", SendWhenEmpty = true }, EmailServices(email));
        host.WithFiles();

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(Assert.Single(email.Messages).Attachments);
        Assert.Single(host.Run.Deliveries);
    }

    [Fact]
    public async Task SendByEmail_SendFails_FailsWithErrors()
    {
        // Arrange
        var email = new FakeEmailService { Result = Result.Failed(new LocalizedString("Error", "The SMTP server refused the message.")) };
        using var host = new StepTestHost(EmailStep(), new SendByEmailStepSettings { To = "ada@example.com" }, EmailServices(email));
        host.WithFiles(host.CreateFile("a.csv", "first"));

        // Act
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => host.ExecuteAsync(TestContext.Current.CancellationToken));

        // Assert
        Assert.Contains("The SMTP server refused the message.", exception.Message, StringComparison.Ordinal);
        Assert.Empty(host.Run.Deliveries);
    }

    private UploadToFtpStep FtpStep(FakeFtpClientFactory factory)
        => new(factory, _secrets, new PassThroughStringLocalizer<UploadToFtpStep>());

    private UploadToSftpStep SftpStep(FakeSftpClientFactory factory)
        => new(factory, _secrets, new PassThroughStringLocalizer<UploadToSftpStep>());

    private SendToWebApiStep WebApiStep(FakeHttpHandler handler)
        => new(new FakeHttpClientFactory(handler), _secrets, new PassThroughStringLocalizer<SendToWebApiStep>());

    private static SendByEmailStep EmailStep()
        => new(new PassThroughStringLocalizer<SendByEmailStep>());

    private static ServiceProvider EmailServices(IEmailService email)
        => new ServiceCollection().AddSingleton(email).BuildServiceProvider();

    private static List<string> Errors(DataPipelineDescribeContext description)
        => description.Issues.Where(issue => issue.Severity == DataPipelineIssueSeverity.Error).Select(issue => issue.Message).ToList();

    private static void AssertNoSecrets(StepTestHost host, params string[] secrets)
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

    private sealed class FakeRemoteServer
    {
        public Dictionary<string, byte[]> Files { get; } = new(StringComparer.Ordinal);

        public HashSet<string> Folders { get; } = new(StringComparer.Ordinal);

        public Exception ConnectError { get; set; }

        public string Fingerprint { get; set; } = "ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6Og";

        public bool Authenticated { get; set; }

        public bool Disposed { get; set; }

        public string Text(string path) => Encoding.UTF8.GetString(Files[path]);
    }

    private sealed class FakeTransferClient : IDataPipelineFileTransferClient
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

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public FakeHttpClientFactory(HttpMessageHandler handler)
        {
            _handler = handler;
        }

        public HttpClient CreateClient(string name)
        {
            Assert.Equal(SendToWebApiStep.HttpClientName, name);

            return new HttpClient(_handler, disposeHandler: false);
        }
    }

    private sealed class FakeHttpHandler : HttpMessageHandler
    {
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

        public string ResponseBody { get; set; } = string.Empty;

        public List<RecordedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var headers = request.Headers.ToDictionary(header => header.Key, header => string.Join(",", header.Value), StringComparer.OrdinalIgnoreCase);

            Requests.Add(new RecordedRequest(
                request.Method,
                request.RequestUri,
                headers,
                request.Content?.Headers.ContentType?.ToString(),
                request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken)));

            return new HttpResponseMessage(Status) { Content = new StringContent(ResponseBody) };
        }
    }

    private sealed record RecordedRequest(HttpMethod Method, Uri Uri, Dictionary<string, string> Headers, string ContentType, string Body);

    private sealed class FakeEmailService : IEmailService
    {
        public Result Result { get; set; } = Result.Success();

        public List<MailMessage> Messages { get; } = [];

        public List<(string Name, string Content)> Attachments { get; } = [];

        public async Task<Result> SendAsync(MailMessage message, string providerName = null, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);

            foreach (var attachment in message.Attachments)
            {
                using var reader = new StreamReader(attachment.Stream, leaveOpen: true);
                Attachments.Add((attachment.Filename, await reader.ReadToEndAsync(cancellationToken)));
            }

            return Result;
        }
    }
}

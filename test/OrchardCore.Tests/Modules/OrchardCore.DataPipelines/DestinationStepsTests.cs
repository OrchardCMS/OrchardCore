using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.Email;
using OrchardCore.Infrastructure;
using OrchardCore.Tests.Modules.OrchardCore.DataSources;
using static OrchardCore.Tests.Modules.OrchardCore.DataPipelines.DestinationStepAssert;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class DestinationStepsTests
{
    private const string Password = "s3cr3t-P@ss";
    private const string Token = "bearer-token-value";
    private const string ApiKey = "api-key-value";

    private readonly DataPipelineSecrets _secrets = new(new EphemeralDataProtectionProvider());

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

    private SendToWebApiStep WebApiStep(FakeHttpHandler handler)
        => new(new FakeHttpClientFactory(handler), _secrets, new PassThroughStringLocalizer<SendToWebApiStep>());

    private static SendByEmailStep EmailStep()
        => new(new PassThroughStringLocalizer<SendByEmailStep>());

    private static ServiceProvider EmailServices(IEmailService email)
        => new ServiceCollection().AddSingleton(email).BuildServiceProvider();

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

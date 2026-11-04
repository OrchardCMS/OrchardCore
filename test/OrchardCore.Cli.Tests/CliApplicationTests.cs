using System.CommandLine;

namespace OrchardCore.Cli.Tests;

public class CliApplicationTests
{
    [Theory]
    [InlineData(new[] { "context", "add" }, "Missing required arguments: <name>, <url>.")]
    [InlineData(new[] { "context", "add", "--current" }, "Missing required arguments: <name>, <url>.")]
    [InlineData(new[] { "context", "add", "production" }, "Missing required argument: <url>.")]
    public async Task InvokeAsync_MissingContextArguments_ReportsNamesOnceWithoutNetwork(string[] args, string expected)
    {
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(InvokeAsync_MissingContextArguments_ReportsNamesOnceWithoutNetwork)));
        var handler = new RequestCountingHandler();
        using var client = new HttpClient(handler);
        using var errors = new StringWriter();
        var app = await CliApplication.CreateAsync(args, paths, client, TestContext.Current.CancellationToken, new UnsupportedCredentialStore());

        Assert.Equal(1, await app.InvokeAsync(args, errors));
        Assert.Equal(expected, errors.ToString().Trim());
        Assert.Equal(0, handler.RequestCount);
        Assert.Empty((await new ContextStore(paths).LoadAsync(TestContext.Current.CancellationToken)).Contexts);
    }

    [Fact]
    public async Task InvokeAsync_ContextAddHelp_DoesNotRequireArguments()
    {
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(InvokeAsync_ContextAddHelp_DoesNotRequireArguments)));
        var handler = new RequestCountingHandler();
        using var client = new HttpClient(handler);
        using var errors = new StringWriter();
        var args = new[] { "context", "add", "--help" };
        var app = await CliApplication.CreateAsync(args, paths, client, TestContext.Current.CancellationToken, new UnsupportedCredentialStore());

        Assert.Equal(0, await app.InvokeAsync(args, errors));
        Assert.Empty(errors.ToString());
        Assert.Equal(0, handler.RequestCount);
    }

    [Theory]
    [InlineData("--version")]
    [InlineData("doctor")]
    public async Task CreateAsync_LocalDiagnostics_DoesNotContactTenant(string command)
    {
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(CreateAsync_LocalDiagnostics_DoesNotContactTenant)));
        await new ContextStore(paths).SaveAsync(new CliConfiguration
        {
            CurrentContext = "offline",
            Contexts = [new TenantContextRecord { Name = "offline", TenantUrl = "https://offline.example/" }],
        }, CancellationToken.None);
        var handler = new RequestCountingHandler();
        using var client = new HttpClient(handler);
        var args = new[] { "--context", "offline", command };

        var app = await CliApplication.CreateAsync(args, paths, client, CancellationToken.None, new UnsupportedCredentialStore());

        Assert.Equal(0, await app.InvokeAsync(args));
        Assert.Equal(0, handler.RequestCount);
        Assert.DoesNotContain(app.RootCommand.Subcommands, subcommand => subcommand.Name == "version");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task InvokeAsync_NoArguments_ShowsHelpWithoutContactingTenant(bool hasContext, bool hasExpiredCache)
    {
        const string tenant = "https://offline.example/";
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(InvokeAsync_NoArguments_ShowsHelpWithoutContactingTenant)));
        if (hasContext)
        {
            await new ContextStore(paths).SaveAsync(new CliConfiguration
            {
                CurrentContext = "offline",
                Contexts = [new TenantContextRecord { Name = "offline", TenantUrl = tenant }],
            }, CancellationToken.None);
        }

        if (hasExpiredCache)
        {
            await new CacheService(paths).WriteAsync(tenant, CacheKind.OpenApi, new CachedContentRecord
            {
                Content = """{"paths":{}}""",
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            }, CancellationToken.None);
        }

        var handler = new RequestCountingHandler();
        using var client = new HttpClient(handler);
        var app = await CliApplication.CreateAsync([], paths, client, CancellationToken.None, new UnsupportedCredentialStore());

        Assert.Equal(0, await app.InvokeAsync([]));
        Assert.Equal(0, handler.RequestCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateAsync_NoArguments_IncludesSameCachedTenantCommandsAsHelp(bool expired)
    {
        const string tenant = "https://offline.example/";
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(CreateAsync_NoArguments_IncludesSameCachedTenantCommandsAsHelp)));
        await new ContextStore(paths).SaveAsync(new CliConfiguration
        {
            CurrentContext = "default",
            Contexts = [new TenantContextRecord { Name = "default", TenantUrl = tenant }],
        }, TestContext.Current.CancellationToken);
        await new CacheService(paths).WriteAsync(tenant, CacheKind.OpenApi, new CachedContentRecord
        {
            Content = """{"paths":{"/api/tenants":{"get":{"operationId":"ListTenants","x-oc-cli":{"commandGroup":["tenants"],"verb":"list"}}}}}""",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(expired ? -5 : 5),
        }, TestContext.Current.CancellationToken);
        var handler = new RequestCountingHandler();
        using var client = new HttpClient(handler);
        var bare = await CliApplication.CreateAsync([], paths, client, TestContext.Current.CancellationToken, new UnsupportedCredentialStore());
        var help = await CliApplication.CreateAsync(["--help"], paths, client, TestContext.Current.CancellationToken, new UnsupportedCredentialStore());

        var tenants = Assert.Single(bare.RootCommand.Subcommands, command => command.Name == "tenants");
        Assert.Contains(tenants.Subcommands, command => command.Name == "list");
        Assert.Equal(help.RootCommand.Subcommands.Select(command => command.Name), bare.RootCommand.Subcommands.Select(command => command.Name));
        Assert.Equal(0, await bare.InvokeAsync([]));
        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public async Task InvokeAsync_ContextRequiredWithoutSelection_WritesFriendlyErrorAndReturnsFailure()
    {
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(InvokeAsync_ContextRequiredWithoutSelection_WritesFriendlyErrorAndReturnsFailure)));
        using var httpClient = new HttpClient();
        using var errorWriter = new StringWriter();
        var args = new[] { "login" };
        var app = await CliApplication.CreateAsync(args, paths, httpClient, CancellationToken.None, new UnsupportedCredentialStore());

        var exitCode = await app.InvokeAsync(args, errorWriter);

        Assert.Equal(1, exitCode);
        Assert.Equal(
            "Error: No context is selected. Add one with 'pomi context add <name> <url>'.",
            errorWriter.ToString().Trim());
        Assert.DoesNotContain("Unhandled exception", errorWriter.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(nameof(CliException), errorWriter.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvokeAsync_ContextClearForce_RemovesAllContexts()
    {
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(InvokeAsync_ContextClearForce_RemovesAllContexts)));
        var store = new ContextStore(paths);
        await store.SaveAsync(new CliConfiguration
        {
            CurrentContext = "primary",
            Contexts =
            [
                new TenantContextRecord { Name = "primary", TenantUrl = "https://primary.example.com/" },
                new TenantContextRecord { Name = "secondary", TenantUrl = "https://secondary.example.com/" },
            ],
        }, CancellationToken.None);
        using var httpClient = new HttpClient();
        var args = new[] { "context", "clear", "--force" };
        var app = await CliApplication.CreateAsync(args, paths, httpClient, CancellationToken.None, new UnsupportedCredentialStore());

        var exitCode = await app.InvokeAsync(args);
        var configuration = await store.LoadAsync(CancellationToken.None);

        Assert.Equal(0, exitCode);
        Assert.Null(configuration.CurrentContext);
        Assert.Empty(configuration.Contexts);
    }

    [Fact]
    public async Task CreateAsync_ContextClearWithRootCommandToken_DoesNotRequestDynamicMetadata()
    {
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(CreateAsync_ContextClearWithRootCommandToken_DoesNotRequestDynamicMetadata)));
        var store = new ContextStore(paths);
        await store.SaveAsync(new CliConfiguration
        {
            CurrentContext = "primary",
            Contexts =
            [
                new TenantContextRecord { Name = "primary", TenantUrl = "https://primary.example.com/" },
            ],
        }, CancellationToken.None);
        var handler = new RequestCountingHandler();
        using var httpClient = new HttpClient(handler);
        var args = new[] { "pomi", "context", "clear", "--force" };

        _ = await CliApplication.CreateAsync(args, paths, httpClient, CancellationToken.None, new UnsupportedCredentialStore());

        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public async Task CreateAsync_SecretRequestProperty_ExposesOnlySecretSafeOptions()
    {
        const string tenantUrl = "https://primary.example.com/";
        const string openApi = """
        {
          "paths": {
            "/api/tenants/{tenantName}:setup": {
              "post": {
                "parameters": [
                  {
                    "name": "tenantName",
                    "in": "path",
                    "required": true,
                    "schema": { "type": "string" }
                  }
                ],
                "requestBody": {
                  "required": true,
                  "content": {
                    "application/json": {
                      "schema": {
                        "type": "object",
                        "required": ["siteName", "password"],
                        "properties": {
                          "siteName": { "type": "string" },
                          "password": { "type": "string" },
                          "connectionString": { "type": "string" }
                        }
                      }
                    }
                  }
                },
                "x-oc-cli": {
                  "commandGroup": ["tenants"],
                  "verb": "setup",
                  "arguments": [{ "parameterName": "tenantName", "position": 0 }],
                  "secretProperties": ["password", "connectionString"]
                }
              }
            }
          }
        }
        """;
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(CreateAsync_SecretRequestProperty_ExposesOnlySecretSafeOptions)));
        await new ContextStore(paths).SaveAsync(new CliConfiguration
        {
            CurrentContext = "primary",
            Contexts = [new TenantContextRecord { Name = "primary", TenantUrl = tenantUrl }],
        }, CancellationToken.None);
        await new CacheService(paths).WriteAsync(tenantUrl, CacheKind.OpenApi, new CachedContentRecord
        {
            Url = tenantUrl + "openapi.json",
            Content = openApi,
            FetchedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
        }, CancellationToken.None);

        using var httpClient = new HttpClient();
        var app = await CliApplication.CreateAsync(["tenants", "setup", "--help"], paths, httpClient, CancellationToken.None, new UnsupportedCredentialStore());
        var tenants = Assert.Single(app.RootCommand.Subcommands, command => command.Name == "tenants");
        var setup = Assert.Single(tenants.Subcommands, command => command.Name == "setup");
        var optionNames = setup.Options.Select(option => option.Name).ToArray();

        Assert.DoesNotContain("--password", optionNames);
        Assert.DoesNotContain("--body", optionNames);
        Assert.DoesNotContain("--connection-string", optionNames);
        Assert.Contains("--connection-string-env", optionNames);
        Assert.Contains("--connection-string-file", optionNames);
        Assert.Contains("--connection-string-stdin", optionNames);
        Assert.Contains("--password-env", optionNames);
        Assert.Contains("--password-file", optionNames);
        Assert.Contains("--password-stdin", optionNames);
        Assert.False(setup.Options.Single(option => option.Name == "--site-name").Required);
    }

    [Theory]
    [InlineData("y", true)]
    [InlineData("YES", true)]
    [InlineData("", false)]
    [InlineData("no", false)]
    public async Task ConfirmContextClearAsync_Response_ReturnsExpectedResult(string response, bool expected)
    {
        using var input = new StringReader(response);
        using var promptWriter = new StringWriter();

        var result = await CliApplication.ConfirmContextClearAsync(2, input, promptWriter, CancellationToken.None);

        Assert.Equal(expected, result);
        Assert.Equal("Delete all 2 saved contexts and stored credentials? [y/N] ", promptWriter.ToString());
    }

    [Theory]
    [InlineData("secret\r\n", "secret")]
    [InlineData("secret\n", "secret")]
    [InlineData("secret", "secret")]
    [InlineData("secret\n\n", "secret\n")]
    public void RemoveTrailingLineEnding_Input_RemovesOneLineEnding(string value, string expected)
    {
        Assert.Equal(expected, CliApplication.RemoveTrailingLineEnding(value));
    }

    [Fact]
    public async Task ResolveSecretValueAsync_FileOption_ReadsValueWithoutTrailingLineEnding()
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, "Secret1!\n", TestContext.Current.CancellationToken);
        try
        {
            var environmentOption = new Option<string?>("--password-env");
            var fileOption = new Option<FileInfo?>("--password-file");
            var stdinOption = new Option<bool>("--password-stdin");
            var command = new RootCommand();
            command.Options.Add(environmentOption);
            command.Options.Add(fileOption);
            command.Options.Add(stdinOption);
            var parseResult = command.Parse(["--password-file", path]);

            var value = await CliApplication.ResolveSecretValueAsync(
                parseResult,
                new SecretBodyPropertyOptions
                {
                    Property = new RequestBodyPropertyDefinition { Name = "password", Required = true },
                    EnvironmentVariableOption = environmentOption,
                    FileOption = fileOption,
                    StdinOption = stdinOption,
                },
                TestContext.Current.CancellationToken);

            Assert.Equal("Secret1!", value);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task CreateAsync_ExpiredCachedHelp_DoesNotContactTenant()
    {
        const string tenant = "https://offline.example/";
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(CreateAsync_ExpiredCachedHelp_DoesNotContactTenant)));
        await new ContextStore(paths).SaveAsync(new CliConfiguration
        {
            CurrentContext = "offline",
            Contexts = [new TenantContextRecord { Name = "offline", TenantUrl = tenant }],
        }, CancellationToken.None);
        await new CacheService(paths).WriteAsync(tenant, CacheKind.OpenApi, new CachedContentRecord
        {
            Content = """{"paths":{"/api/items":{"get":{"operationId":"ListItems","x-oc-cli":{"commandGroup":["items"],"verb":"list"}}}}}""",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1),
        }, CancellationToken.None);
        var handler = new RequestCountingHandler();
        using var client = new HttpClient(handler);
        var app = await CliApplication.CreateAsync(["--context=offline", "items", "--help"], paths, client, CancellationToken.None, new UnsupportedCredentialStore());
        Assert.Equal(0, handler.RequestCount);
        Assert.Contains(app.RootCommand.Subcommands, command => command.Name == "items");
    }

    [Fact]
    public async Task ResolveJsonBodyAsync_ConflictingSources_RejectsBeforeReading()
    {
        await Assert.ThrowsAsync<CliException>(() => CliApplication.ResolveJsonBodyAsync("{}", new FileInfo("missing.json"), false, "application/json", CancellationToken.None));
    }

    [Fact]
    public async Task InvokeAsync_InvalidOutput_DoesNotDeleteContexts()
    {
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(InvokeAsync_InvalidOutput_DoesNotDeleteContexts)));
        var store = new ContextStore(paths);
        await store.SaveAsync(new CliConfiguration { CurrentContext = "keep", Contexts = [new TenantContextRecord { Name = "keep", TenantUrl = "https://example.com/" }] }, CancellationToken.None);
        using var client = new HttpClient();
        var args = new[] { "context", "clear", "--force", "--output", "invalid" };
        var app = await CliApplication.CreateAsync(args, paths, client, CancellationToken.None, new UnsupportedCredentialStore());
        Assert.Equal(1, await app.InvokeAsync(args, TextWriter.Null));
        Assert.Single((await store.LoadAsync(CancellationToken.None)).Contexts);
    }

    [Theory]
    [InlineData("https://cms.example.com/", "https://cms.example.com/.well-known/orchardcore-management")]
    [InlineData("https://cms.example.com/blog/", "https://cms.example.com/blog/.well-known/orchardcore-management")]
    public async Task ContextAdd_MissingDiscovery_ExplainsSetupAndPreservesExistingContext(string tenantUrl, string discoveryUrl)
    {
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(ContextAdd_MissingDiscovery_ExplainsSetupAndPreservesExistingContext)));
        var store = new ContextStore(paths);
        await store.SaveAsync(new CliConfiguration
        {
            CurrentContext = "existing",
            Contexts = [new TenantContextRecord { Name = "existing", TenantUrl = "https://existing.example/" }],
        }, TestContext.Current.CancellationToken);
        var handler = new MissingDiscoveryHandler();
        using var client = new HttpClient(handler);
        var args = new[] { "context", "add", "new-site", tenantUrl, "--current" };
        var app = await CliApplication.CreateAsync(args, paths, client, TestContext.Current.CancellationToken, new UnsupportedCredentialStore());
        using var errors = new StringWriter();

        Assert.Equal(1, await app.InvokeAsync(args, errors));
        Assert.Equal(discoveryUrl, Assert.Single(handler.Requests));
        Assert.Contains("Remote Management discovery was not found", errors.ToString());
        Assert.Contains(discoveryUrl, errors.ToString());
        Assert.Contains("HTTP 404", errors.ToString());
        Assert.Contains("path prefix", errors.ToString());
        Assert.Contains("OrchardCore.RemoteManagement", errors.ToString());
        Assert.Contains("Settings > Remote Management", errors.ToString());
        Assert.DoesNotContain("Response status code does not indicate success", errors.ToString());
        var saved = await store.LoadAsync(TestContext.Current.CancellationToken);
        Assert.Equal("existing", saved.CurrentContext);
        Assert.Equal("existing", Assert.Single(saved.Contexts).Name);
    }

    private sealed class MissingDiscoveryHandler : HttpMessageHandler
    {
        public List<string> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!.AbsoluteUri);
            Assert.Null(request.Headers.Authorization);
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        }
    }

    private sealed class RequestCountingHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
        }
    }

}

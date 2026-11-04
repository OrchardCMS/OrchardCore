using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.Cli.Tests;

public class DeviceLoginSessionTests
{
    private const string Tenant = "https://cms.example/blog/";
    private const string Grant = "urn:ietf:params:oauth:grant-type:device_code";

    [Fact]
    public async Task Start_SavesPrivateSessionWithoutPolling_AndShowWorksOfflineDuringWait()
    {
        var (paths, context) = await CreateContextAsync();
        await new ContextStore(paths).SaveAsync(new CliConfiguration
        {
            CurrentContext = "other",
            Contexts = [context, new TenantContextRecord { Name = "other", TenantUrl = "https://other.example/" }],
        }, TestContext.Current.CancellationToken);
        var handler = new Handler(request => request.RequestUri!.AbsolutePath.EndsWith("openid-configuration", StringComparison.Ordinal)
            ? Ok($$"""{"issuer":"{{Tenant}}","token_endpoint":"{{Tenant}}connect/token","device_authorization_endpoint":"{{Tenant}}connect/device"}""")
            : Ok($$"""{"device_code":"private-code","user_code":"1234-5678","verification_uri":"{{Tenant}}connect/verify","expires_in":600,"interval":2}"""));
        using var http = new HttpClient(handler);
        Assert.Equal(0, await InvokeAsync(paths, http, ["login", "blog", "device", "start", "--output", "none"]));
        Assert.Equal(2, handler.Count);
        var file = Assert.Single(Directory.GetFiles(Path.Combine(paths.RootDirectory, "device-logins"), "*.json"));
        var store = new DeviceLoginSessionStore(paths);
        var id = Path.GetFileNameWithoutExtension(file);
        var session = await store.ReadAsync(id, TestContext.Current.CancellationToken);
        Assert.Equal("private-code", session.DeviceCode);
        Assert.Equal(context.ClientId, session.ClientId);
        Assert.Equal(context.TenantUrl, session.TenantUrl);
        Assert.Equal(2, session.IntervalSeconds);
        Assert.DoesNotContain("private-code", session.ToPublicJson(true).ToJsonString());
        if (!OperatingSystem.IsWindows())
        {
            Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite, File.GetUnixFileMode(file));
            Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute, File.GetUnixFileMode(Path.GetDirectoryName(file)!));
        }

        using var lease = store.Acquire(id);
        using var offline = new HttpClient(new Handler(_ => throw new InvalidOperationException("Show must stay offline.")));
        Assert.Equal(0, await InvokeAsync(paths, offline, ["login", "device", "show", id, "--output", "none"]));
        var error = Assert.Throws<CliException>(() => store.Acquire(id));
        Assert.Contains("already waiting", error.Message);
        Assert.Equal("private-code", (await store.ReadAsync(id, TestContext.Current.CancellationToken)).DeviceCode);
    }

    [Theory]
    [InlineData("access_denied")]
    [InlineData("expired_token")]
    [InlineData("invalid_grant")]
    [InlineData("locally_expired")]
    public async Task Wait_TerminalFailure_DeletesSessionWithoutSavingCredentials(string error)
    {
        var (paths, context) = await CreateContextAsync();
        var session = Session(context);
        if (error == "locally_expired")
        {
            session.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-1);
        }
        var store = new DeviceLoginSessionStore(paths);
        await store.SaveAsync(session, TestContext.Current.CancellationToken);
        var handler = new Handler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(new JsonObject { ["error"] = error }.ToJsonString()) });
        using var http = new HttpClient(handler);
        Assert.Equal(1, await InvokeAsync(paths, http, ["login", "device", "wait", session.SessionId, "--output", "none"]));
        Assert.Equal(error == "locally_expired" ? 0 : 1, handler.Count);
        await Assert.ThrowsAsync<CliException>(() => store.ReadAsync(session.SessionId, TestContext.Current.CancellationToken));
        Assert.Null(await new FileCredentialStore(paths).GetAsync(CliApplication.GetCredentialKey(context), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Wait_Success_BindsOriginalContextAndSavesTokenBeforeRefreshingMetadata()
    {
        var (paths, context) = await CreateContextAsync();
        var configuration = await new ContextStore(paths).LoadAsync(TestContext.Current.CancellationToken);
        configuration.Contexts.Add(new TenantContextRecord { Name = "other", TenantUrl = "https://other.example/" });
        configuration.CurrentContext = "other";
        await new ContextStore(paths).SaveAsync(configuration, TestContext.Current.CancellationToken);
        var session = Session(context);
        var store = new DeviceLoginSessionStore(paths);
        await store.SaveAsync(session, TestContext.Current.CancellationToken);
        var handler = new Handler(request =>
        {
            Assert.Equal("cms.example", request.RequestUri!.Host);
            if (request.RequestUri.AbsolutePath.EndsWith("openid-configuration", StringComparison.Ordinal))
            {
                return Ok($$"""{"issuer":"{{Tenant}}","token_endpoint":"{{Tenant}}connect/token"}""");
            }
            if (request.Method == HttpMethod.Post)
            {
                return Ok("""{"access_token":"private-token","expires_in":3600}""");
            }
            Assert.Equal("private-token", request.Headers.Authorization?.Parameter);
            Assert.False(File.Exists(Path.Combine(paths.RootDirectory, "device-logins", session.SessionId + ".json")));
            return request.RequestUri.AbsolutePath.EndsWith("manifest", StringComparison.Ordinal)
                ? Ok($$$"""{"protocolMajorVersion":1,"protocolMinorVersion":0,"managementManifestUrl":"{{{Tenant}}}api/management/manifest","openApiUrl":"{{{Tenant}}}openapi.json","authentication":{"authority":"{{{Tenant}}}","clientId":"orchardcore-cli","grantTypes":["{{{Grant}}}"],"scopes":[]}}""")
                : Ok("""{"openapi":"3.0.0","paths":{}}""");
        });
        using var http = new HttpClient(handler);
        Assert.Equal(0, await InvokeAsync(paths, http, ["login", "device", "wait", session.SessionId, "--output", "none"]));
        Assert.Equal("private-token", (await new FileCredentialStore(paths).GetAsync(CliApplication.GetCredentialKey(context), TestContext.Current.CancellationToken))?.AccessToken);
        Assert.Equal("other", (await new ContextStore(paths).LoadAsync(TestContext.Current.CancellationToken)).CurrentContext);
    }

    [Theory]
    [InlineData("client")]
    [InlineData("tenant")]
    [InlineData("authority")]
    [InlineData("scopes")]
    [InlineData("explicit_context")]
    [InlineData("positional_context")]
    public async Task Wait_ChangedContext_RejectsBeforeNetwork(string change)
    {
        var (paths, context) = await CreateContextAsync();
        var session = Session(context);
        await new DeviceLoginSessionStore(paths).SaveAsync(session, TestContext.Current.CancellationToken);
        switch (change)
        {
            case "client": context.ClientId = "another-client"; break;
            case "tenant": context.TenantUrl = "https://cms.example/other/"; break;
            case "authority": context.Authority = "https://elsewhere.example/"; break;
            case "scopes": context.Scopes.Add("admin"); break;
        }
        await new ContextStore(paths).SaveAsync(new CliConfiguration { CurrentContext = context.Name, Contexts = [context] }, TestContext.Current.CancellationToken);
        using var http = new HttpClient(new Handler(_ => throw new InvalidOperationException("Changed context must not send credentials.")));
        string[] args = ["login", "device", "wait", session.SessionId, "--output", "none"];
        if (change == "explicit_context")
        {
            args = [.. args, "--context", "other"];
        }
        else if (change == "positional_context")
        {
            args = ["login", "other", "device", "wait", session.SessionId, "--output", "none"];
        }
        Assert.Equal(1, await InvokeAsync(paths, http, args));
    }

    [Fact]
    public async Task Wait_ContextChangesDuringPolling_DoesNotSaveTokenOrOverwriteContext()
    {
        var (paths, context) = await CreateContextAsync();
        var session = Session(context);
        var store = new DeviceLoginSessionStore(paths);
        await store.SaveAsync(session, TestContext.Current.CancellationToken);
        using var http = new HttpClient(new Handler(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            var changed = new CliConfiguration
            {
                CurrentContext = context.Name,
                Contexts = [new TenantContextRecord { Name = context.Name, TenantUrl = "https://other.example/", ClientId = "different-client" }],
            };
            File.WriteAllText(paths.ConfigFilePath, JsonSerializer.Serialize(changed, CliJsonContext.Default.CliConfiguration));
            return Ok("""{"access_token":"private-token","expires_in":3600}""");
        }));
        Assert.Equal(1, await InvokeAsync(paths, http, ["login", "device", "wait", session.SessionId, "--output", "none"]));
        Assert.Null(await new FileCredentialStore(paths).GetAsync(CliApplication.GetCredentialKey(context), TestContext.Current.CancellationToken));
        Assert.Equal("https://other.example/", (await new ContextStore(paths).LoadAsync(TestContext.Current.CancellationToken)).Contexts[0].TenantUrl);
        await Assert.ThrowsAsync<CliException>(() => store.ReadAsync(session.SessionId, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Store_LinkedSession_RejectsWithoutReadingOrOverwritingTarget()
    {
        if (OperatingSystem.IsWindows())
        {
            return; // Windows symlink creation requires permissions not available on every runner.
        }
        var (paths, context) = await CreateContextAsync();
        var session = Session(context);
        var store = new DeviceLoginSessionStore(paths);
        await store.SaveAsync(session, TestContext.Current.CancellationToken);
        var statePath = Path.Combine(paths.RootDirectory, "device-logins", session.SessionId + ".json");
        var target = Path.Combine(paths.RootDirectory, "unrelated.txt");
        await File.WriteAllTextAsync(target, "unrelated content", TestContext.Current.CancellationToken);
        File.Delete(statePath);
        File.CreateSymbolicLink(statePath, target);
        await Assert.ThrowsAsync<CliException>(() => store.ReadAsync(session.SessionId, TestContext.Current.CancellationToken));
        Assert.Equal("unrelated content", await File.ReadAllTextAsync(target, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Wait_SlowDownAndInterruption_PreservesPollingStateForResume()
    {
        var (paths, context) = await CreateContextAsync();
        var session = Session(context);
        var store = new DeviceLoginSessionStore(paths);
        await store.SaveAsync(session, TestContext.Current.CancellationToken);
        using var cancellation = new CancellationTokenSource();
        var handler = new Handler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("""{"error":"slow_down"}""") });
        using var http = new HttpClient(handler);
        var oauth = new OAuthClient(http, TextWriter.Null);
        var saves = 0;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => oauth.WaitForDeviceAuthorizationAsync(session, cancellation.Token, async ct =>
        {
            await store.SaveAsync(session, ct);
            if (++saves == 2)
            {
                cancellation.Cancel();
            }
        }));
        var resumed = await store.ReadAsync(session.SessionId, TestContext.Current.CancellationToken);
        Assert.Equal(6, resumed.IntervalSeconds);
        Assert.True(resumed.NextPollAt > DateTimeOffset.UtcNow.AddSeconds(4));
        Assert.Equal(1, handler.Count);
        using var resumeCancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => oauth.WaitForDeviceAuthorizationAsync(resumed, resumeCancellation.Token));
        Assert.Equal(1, handler.Count);
    }

    [Fact]
    public async Task Wait_TransientHttpFailure_KeepsSession()
    {
        var (paths, context) = await CreateContextAsync();
        var session = Session(context);
        var store = new DeviceLoginSessionStore(paths);
        await store.SaveAsync(session, TestContext.Current.CancellationToken);
        using var http = new HttpClient(new Handler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = new StringContent("Temporarily unavailable") }));
        Assert.Equal(2, await InvokeAsync(paths, http, ["login", "device", "wait", session.SessionId, "--output", "none"]));
        Assert.True((await store.ReadAsync(session.SessionId, TestContext.Current.CancellationToken)).NextPollAt > session.NextPollAt);
    }

    [Theory]
    [InlineData("../credentials")]
    [InlineData("1234-5678")]
    [InlineData("")]
    public async Task Store_InvalidIds_RejectsPathsAndUserCodes(string id)
    {
        var store = new DeviceLoginSessionStore(new CliPaths(TestPaths.CreateScratchDirectory(nameof(Store_InvalidIds_RejectsPathsAndUserCodes))));
        await Assert.ThrowsAsync<CliException>(() => store.ReadAsync(id, TestContext.Current.CancellationToken));
        Assert.Throws<CliException>(() => store.Acquire(id));
    }

    [Fact]
    public async Task Store_PruneExpired_LeavesActiveAndLockedSessions()
    {
        var (paths, context) = await CreateContextAsync();
        var store = new DeviceLoginSessionStore(paths);
        var expired = Session(context);
        expired.ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1);
        var locked = Session(context);
        locked.ExpiresAt = expired.ExpiresAt;
        var active = Session(context);
        foreach (var session in new[] { expired, locked, active })
        {
            await store.SaveAsync(session, TestContext.Current.CancellationToken);
        }
        using var lease = store.Acquire(locked.SessionId);
        await store.PruneExpiredAsync(TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<CliException>(() => store.ReadAsync(expired.SessionId, TestContext.Current.CancellationToken));
        Assert.NotNull(await store.ReadAsync(locked.SessionId, TestContext.Current.CancellationToken));
        Assert.NotNull(await store.ReadAsync(active.SessionId, TestContext.Current.CancellationToken));
    }

    private static DeviceLoginSession Session(TenantContextRecord context) => new()
    {
        ContextName = context.Name, TenantUrl = context.TenantUrl, ClientId = context.ClientId!, Scopes = [.. context.Scopes],
        Issuer = Tenant, TokenEndpoint = Tenant + "connect/token", VerificationUri = Tenant + "connect/verify", UserCode = "1234-5678",
        DeviceCode = "private-code", IntervalSeconds = 1, NextPollAt = DateTimeOffset.UtcNow.AddSeconds(-1), ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
    };

    private static async Task<(CliPaths, TenantContextRecord)> CreateContextAsync()
    {
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(DeviceLoginSessionTests)));
        var context = new TenantContextRecord { Name = "blog", TenantUrl = Tenant, Authority = Tenant, ClientId = "orchardcore-cli", GrantTypes = [Grant] };
        await new ContextStore(paths).SaveAsync(new CliConfiguration { CurrentContext = context.Name, Contexts = [context] }, TestContext.Current.CancellationToken);
        return (paths, context);
    }

    private static async Task<int> InvokeAsync(CliPaths paths, HttpClient http, string[] args)
    {
        var app = await CliApplication.CreateAsync(args, paths, http, TestContext.Current.CancellationToken, new FileCredentialStore(paths));
        using var errors = new StringWriter();
        return await app.InvokeAsync(args, errors);
    }

    private static HttpResponseMessage Ok(string body) => new(HttpStatusCode.OK) { Content = new StringContent(body) };

    private sealed class Handler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _response;
        public int Count { get; private set; }
        public Handler(Func<HttpRequestMessage, HttpResponseMessage> response) => _response = response;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Count++;
            return Task.FromResult(_response(request));
        }
    }
}

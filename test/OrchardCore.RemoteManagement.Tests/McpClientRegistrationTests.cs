using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.OpenId;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Controllers;
using OrchardCore.OpenId.Handlers;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;
using ISession = YesSql.ISession;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace OrchardCore.RemoteManagement.Tests;

public class McpClientRegistrationTests
{
    [Theory]
    [InlineData(false, 201, 1)]
    [InlineData(true, 503, 0)]
    public async Task Registration_AtCapacity_RemovesOnlyOldUnauthorizedClients(bool authorized, int status, int removed)
    {
        var fixture = new Fixture();
        fixture.Options.Value.MaximumApplications = 1;
        fixture.AddRegisteredClients(1, DateTimeOffset.UtcNow.AddDays(-2));
        fixture.Authorizations.Setup(manager => manager.FindByApplicationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() => (authorized ? new[] { new object() } : []).ToAsyncEnumerable());
        using var limiter = new McpClientRegistrationLimiter();

        var result = await fixture.CreateController(limiter).Register(ValidRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(status, ((ObjectResult)result).StatusCode);
        fixture.Applications.Verify(manager => manager.DeleteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Exactly(removed));
    }

    [Fact]
    public async Task Registration_AtCapacity_KeepsRecentClients()
    {
        var fixture = new Fixture();
        fixture.Options.Value.MaximumApplications = 1;
        fixture.AddRegisteredClients(1, DateTimeOffset.UtcNow);
        using var limiter = new McpClientRegistrationLimiter();

        var result = await fixture.CreateController(limiter).Register(ValidRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(503, ((ObjectResult)result).StatusCode);
        fixture.Applications.Verify(manager => manager.DeleteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Options_ShellConfiguration_DisablesRegistration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["OpenId:Mcp:AllowDynamicClientRegistration"] = "false" })
            .Build();
        var services = new ServiceCollection();
        services.AddSingleton<IShellConfiguration>(new ShellConfiguration(configuration));
        new RemoteManagementMcpStartup().ConfigureServices(services);

        using var provider = services.BuildServiceProvider();

        Assert.False(provider.GetRequiredService<IOptions<RemoteManagementMcpOptions>>().Value.AllowDynamicClientRegistration);
    }

    [Fact]
    public async Task Registration_CreatesDistinctPublicClientsWithoutRolesOrSecrets()
    {
        var fixture = new Fixture();
        var request = ValidRequest();
        request.GrantTypes = ["authorization_code", "refresh_token"];
        request.Scope = "openid profile offline_access orchardcore.management";
        var first = await fixture.Service.RegisterAsync(request, TestContext.Current.CancellationToken);
        var second = await fixture.Service.RegisterAsync(request, TestContext.Current.CancellationToken);
        Assert.NotEqual(first.ClientId, second.ClientId);
        Assert.StartsWith("orchardcore-mcp-", first.ClientId);
        Assert.Equal(OpenIddictConstants.ClientTypes.Public, first.ClientType);
        Assert.Equal(OpenIddictConstants.ConsentTypes.Explicit, first.ConsentType);
        Assert.Contains(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange, first.Requirements);
        Assert.Contains(OpenIddictConstants.Permissions.GrantTypes.RefreshToken, first.Permissions);
        Assert.DoesNotContain(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials, first.Permissions);
        Assert.DoesNotContain(OpenIddictConstants.Permissions.GrantTypes.Password, first.Permissions);
        Assert.Contains(OpenIddictConstants.Permissions.Prefixes.Resource + "https://site.example/tenant-a/mcp", first.Permissions);
        Assert.Empty(first.Roles);
        Assert.Null(first.ClientSecret);
        Assert.Equal(new Uri("https://client.example/callback"), Assert.Single(first.RedirectUris));
        fixture.Session.Verify(session => session.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        fixture.Applications.Verify(manager => manager.UpdateAsync(It.IsAny<object>(), It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("http://remote.example/callback")]
    [InlineData("https://client.example/*")]
    [InlineData("https://client.example/callback#fragment")]
    [InlineData("https://user:password@client.example/callback")]
    [InlineData("javascript:alert(1)")]
    [InlineData("https://client.example/callback\nhttps://another.example/callback")]
    [InlineData(null)]
    public async Task Registration_InvalidCallback_DoesNotWrite(string callback)
    {
        var fixture = new Fixture();
        var request = ValidRequest();
        request.RedirectUris = [callback];
        await Assert.ThrowsAnyAsync<ValidationException>(() => fixture.Service.RegisterAsync(request, TestContext.Current.CancellationToken));
        fixture.Applications.VerifyNoOtherCalls();
        fixture.Session.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("http://127.0.0.1:12345/callback")]
    [InlineData("http://[::1]:12345/callback")]
    [InlineData("http://localhost:12345/callback")]
    public async Task Registration_AllowsLoopbackCallbacks(string callback)
    {
        var fixture = new Fixture();
        var request = ValidRequest();
        request.RedirectUris = [callback];
        var application = await fixture.Service.RegisterAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(OpenIddictConstants.ApplicationTypes.Native, application.ApplicationType);
    }

    [Theory]
    [InlineData("client_credentials", "none", "orchardcore.management")]
    [InlineData("authorization_code", "client_secret_post", "orchardcore.management")]
    [InlineData("authorization_code", "none", "administrator")]
    [InlineData("authorization_code", "none", "openid")]
    public async Task Registration_InvalidGrantMethodOrScope_DoesNotWrite(string grant, string method, string scope)
    {
        var fixture = new Fixture();
        var request = ValidRequest();
        request.GrantTypes = [grant];
        request.TokenEndpointAuthMethod = method;
        request.Scope = scope;
        await Assert.ThrowsAnyAsync<ValidationException>(() => fixture.Service.RegisterAsync(request, TestContext.Current.CancellationToken));
        fixture.Applications.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(0, 1000, false, 404)]
    [InlineData(1000, 1000, true, 503)]
    [InlineData(0, 1000, true, 201)]
    public async Task Registration_RespectsDisabledModeAndCapacity(int count, int maximum, bool enabled, int status)
    {
        var fixture = new Fixture();
        fixture.Options.Value.AllowDynamicClientRegistration = enabled;
        fixture.Options.Value.MaximumApplications = maximum;
        fixture.AddRegisteredClients(count, DateTimeOffset.UtcNow);
        using var limiter = new McpClientRegistrationLimiter();
        var controller = fixture.CreateController(limiter);
        var result = await controller.Register(ValidRequest(), TestContext.Current.CancellationToken);
        Assert.Equal(status, result is StatusCodeResult statusResult ? statusResult.StatusCode : ((ObjectResult)result).StatusCode);
        Assert.Equal("no-store", controller.Response.Headers.CacheControl);
        if (status != 201)
        {
            fixture.Applications.Verify(manager => manager.CreateAsync(It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Fact]
    public async Task Registration_RequiresConfiguredAuthenticationAndLimitsAnonymousRequests()
    {
        var fixture = new Fixture();
        using var limiter = new McpClientRegistrationLimiter();
        var controller = fixture.CreateController(limiter, ready: false);
        for (var i = 0; i < 10; i++)
        {
            Assert.Equal(503, Assert.IsType<ObjectResult>(await controller.Register(ValidRequest(), TestContext.Current.CancellationToken)).StatusCode);
        }

        Assert.Equal(429, Assert.IsType<ObjectResult>(await controller.Register(ValidRequest(), TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal("60", controller.Response.Headers.RetryAfter);
        fixture.Applications.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Registration_OneAddressOverItsLimit_DoesNotLockOutOthers()
    {
        var fixture = new Fixture();
        using var limiter = new McpClientRegistrationLimiter();
        var attacker = fixture.CreateController(limiter, ready: false);
        attacker.HttpContext.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.1");
        for (var i = 0; i < 10; i++)
        {
            Assert.Equal(503, Assert.IsType<ObjectResult>(await attacker.Register(ValidRequest(), TestContext.Current.CancellationToken)).StatusCode);
        }

        Assert.Equal(429, Assert.IsType<ObjectResult>(await attacker.Register(ValidRequest(), TestContext.Current.CancellationToken)).StatusCode);

        var other = fixture.CreateController(limiter, ready: false);
        other.HttpContext.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.2");

        Assert.Equal(503, Assert.IsType<ObjectResult>(await other.Register(ValidRequest(), TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public void Limiter_ManyAddresses_StopsAtTheTenantCeiling()
    {
        using var limiter = new McpClientRegistrationLimiter();
        var acquired = 0;
        for (var i = 0; i < 150; i++)
        {
            using var lease = limiter.AttemptAcquire("address-" + i);
            acquired += lease.IsAcquired ? 1 : 0;
        }

        Assert.Equal(100, acquired);
    }

    [Fact]
    public void Limiter_AddressOverItsLimit_DoesNotConsumeTheTenantCeiling()
    {
        using var limiter = new McpClientRegistrationLimiter();
        for (var i = 0; i < 500; i++)
        {
            using var lease = limiter.AttemptAcquire("attacker");
        }

        var acquired = 0;
        for (var i = 0; i < 90; i++)
        {
            using var lease = limiter.AttemptAcquire("address-" + i);
            acquired += lease.IsAcquired ? 1 : 0;
        }

        Assert.Equal(90, acquired);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    public async Task Discovery_AdvertisesTenantRegistrationOnlyWhenEnabledAndReady(bool enabled, bool ready, bool advertised)
    {
        var fixture = new Fixture();
        fixture.Options.Value.AllowDynamicClientRegistration = enabled;
        var context = new HandleConfigurationRequestContext(Transaction());
        await new McpRegistrationMetadataHandler(Configuration(ready), fixture.Options).HandleAsync(context);
        Assert.Equal(advertised, context.Metadata.ContainsKey("registration_endpoint"));
        if (advertised)
        {
            Assert.Equal("https://site.example/tenant-a/connect/mcp/register", (string)context.Metadata["registration_endpoint"]);
        }
    }

    [Theory]
    [InlineData("S256", "https://site.example/tenant-a/mcp", false)]
    [InlineData("plain", "https://site.example/tenant-a/mcp", true)]
    [InlineData(null, "https://site.example/tenant-a/mcp", true)]
    [InlineData("S256", "https://site.example/tenant-b/mcp", true)]
    [InlineData("S256", null, true)]
    public async Task DynamicAuthorization_RequiresS256AndTheCorrectResource(string challenge, string resource, bool rejected)
    {
        var fixture = new Fixture();
        fixture.AddDynamicClient();
        var transaction = Transaction();
        transaction.Request.CodeChallengeMethod = challenge;
        transaction.Request["resource"] = resource;
        var sharedOptions = transaction.Options;
        var context = new ValidateAuthorizationRequestContext(transaction);
        await new McpAuthorizationRequestHandler(fixture.Applications.Object, fixture.OptionsFactory.Object).HandleAsync(context);
        Assert.Equal(rejected, context.IsRejected);
        Assert.Empty(sharedOptions.Resources);
        if (!rejected)
        {
            Assert.NotSame(sharedOptions, context.Options);
            Assert.Contains(new Uri("https://site.example/tenant-a/mcp"), context.Options.Resources);
            Assert.False(context.Options.DisableResourceValidation);
            Assert.False(context.Options.IgnoreResourcePermissions);
        }
    }

    [Theory]
    [InlineData("https://site.example/tenant-a/mcp", false)]
    [InlineData("https://site.example/tenant-b/mcp", true)]
    [InlineData(null, true)]
    public async Task DynamicTokenRequest_RequiresTheCorrectResource(string resource, bool rejected)
    {
        var fixture = new Fixture();
        fixture.AddDynamicClient();
        var transaction = Transaction();
        transaction.Request["resource"] = resource;
        var context = new ValidateTokenRequestContext(transaction);
        await new McpTokenRequestHandler(fixture.Applications.Object, fixture.OptionsFactory.Object).HandleAsync(context);
        Assert.Equal(rejected, context.IsRejected);
    }

    [Fact]
    public async Task DynamicSignIn_AddsMcpAudienceAndPreservesExistingResources()
    {
        var fixture = new Fixture();
        fixture.AddDynamicClient();
        var principal = new ClaimsPrincipal(new ClaimsIdentity("test"));
        principal.SetResources("orchardcore", "tenant-a");
        var context = new ProcessSignInContext(Transaction()) { Principal = principal };
        await new McpTokenResourceHandler(fixture.Applications.Object).HandleAsync(context);
        Assert.Contains("https://site.example/tenant-a/mcp", principal.GetResources());
        Assert.Contains("orchardcore", principal.GetResources());
        Assert.Contains("tenant-a", principal.GetResources());
    }

    [Fact]
    public async Task ExistingClients_KeepTheirAuthorizationBehavior()
    {
        var fixture = new Fixture();
        var context = new ValidateAuthorizationRequestContext(Transaction());
        await new McpAuthorizationRequestHandler(fixture.Applications.Object, fixture.OptionsFactory.Object).HandleAsync(context);
        Assert.False(context.IsRejected);
        fixture.OptionsFactory.VerifyNoOtherCalls();
    }

    private static McpClientRegistrationRequest ValidRequest() => new() { ClientName = "Test client", RedirectUris = ["https://client.example/callback"] };

    private static OpenIddictServerTransaction Transaction() => new()
    {
        BaseUri = new Uri("https://site.example/tenant-a"),
        Request = new OpenIddictRequest { ClientId = "test-client" },
        Options = new OpenIddictServerOptions(),
    };

    private static RemoteManagementConfigurationService Configuration(bool ready)
    {
        var server = new Mock<IOpenIdServerService>();
        server.Setup(service => service.GetSettingsAsync()).ReturnsAsync(new OpenIdServerSettings
        {
            AuthorizationEndpointPath = "/connect/authorize", TokenEndpointPath = "/connect/token",
            LogoutEndpointPath = "/connect/logout", RevocationEndpointPath = "/connect/revoke",
            AllowAuthorizationCodeFlow = ready, AllowClientCredentialsFlow = true,
            AllowRefreshTokenFlow = true, RequireProofKeyForCodeExchange = true,
        });
        var validation = new Mock<IOpenIdValidationService>();
        validation.Setup(service => service.GetSettingsAsync()).ReturnsAsync(new OpenIdValidationSettings { Tenant = "tenant-a" });
        var scope = new object();
        var scopes = new Mock<IOpenIdScopeManager>();
        scopes.Setup(manager => manager.FindByNameAsync("orchardcore.management", It.IsAny<CancellationToken>())).ReturnsAsync(scope);
        scopes.Setup(manager => manager.GetResourcesAsync(scope, It.IsAny<CancellationToken>())).ReturnsAsync(ImmutableArray.Create("orchardcore"));
        return new RemoteManagementConfigurationService(scopes.Object, server.Object, validation.Object, new ShellSettings { Name = "tenant-a" });
    }

    private sealed class Fixture
    {
        public Mock<IOpenIdApplicationManager> Applications { get; } = new();
        public Mock<IOpenIdAuthorizationManager> Authorizations { get; } = new();
        public Mock<IOptionsFactory<OpenIddictServerOptions>> OptionsFactory { get; } = new();
        public Mock<ISession> Session { get; } = new();
        public IOptions<RemoteManagementMcpOptions> Options { get; } = Microsoft.Extensions.Options.Options.Create(new RemoteManagementMcpOptions());
        public McpClientRegistrationService Service { get; }
        private readonly List<object> _registered = [];

        public Fixture()
        {
            OptionsFactory.Setup(factory => factory.Create(It.IsAny<string>())).Returns(() => new OpenIddictServerOptions());
            var distributedLock = new Mock<IDistributedLock>();
            distributedLock.Setup(value => value.TryAcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan?>()))
                .ReturnsAsync((Mock.Of<ILocker>(), true));
            Applications.Setup(manager => manager.ListAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .Returns(() => _registered.ToAsyncEnumerable());
            Service = new McpClientRegistrationService(Applications.Object, Authorizations.Object, distributedLock.Object, Session.Object, Options, new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext
                {
                    Request = { Scheme = "https", Host = new HostString("site.example"), PathBase = "/tenant-a" },
                },
            });
        }

        public McpClientRegistrationController CreateController(McpClientRegistrationLimiter limiter, bool ready = true) =>
            new(Service, limiter, Configuration(ready), Options)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            };

        public void AddRegisteredClients(int count, DateTimeOffset registeredAt)
        {
            var properties = ImmutableDictionary.CreateRange(new Dictionary<string, JsonElement>
            {
                ["orchardcore:remote-management:dynamic-mcp"] = JsonSerializer.SerializeToElement(true),
                ["orchardcore:remote-management:registered-at"] = JsonSerializer.SerializeToElement(registeredAt.ToUnixTimeSeconds()),
            });
            for (var i = 0; i < count; i++)
            {
                var application = new object();
                _registered.Add(application);
                Applications.Setup(manager => manager.GetPropertiesAsync(application, It.IsAny<CancellationToken>())).ReturnsAsync(properties);
                Applications.Setup(manager => manager.GetIdAsync(application, It.IsAny<CancellationToken>())).ReturnsAsync("id-" + i);
            }
        }

        public void AddDynamicClient()
        {
            var application = new object();
            Applications.Setup(manager => manager.FindByClientIdAsync("test-client", It.IsAny<CancellationToken>())).ReturnsAsync(application);
            Applications.Setup(manager => manager.GetPropertiesAsync(application, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ImmutableDictionary<string, JsonElement>.Empty.Add("orchardcore:remote-management:dynamic-mcp", JsonSerializer.SerializeToElement(true)));
        }
    }
}

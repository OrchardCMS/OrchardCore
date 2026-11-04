using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.BackgroundTasks;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.AutoSetup.Options;
using OrchardCore.AutoSetup.Services;
using OrchardCore.Setup.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.RemoteManagement;
using OrchardCore.Tenants.AuditTrail;
using OrchardCore.Tenants.Controllers;
using OrchardCore.Tenants.Endpoints.Management;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class ClientProvisioningApiTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AutoSetup_BlankRecipe_ProvisionsOnlyWhenCredentialsAreProvided(bool provision)
    {
        var name = Guid.NewGuid().ToString("N");
        using var created = await SiteContext.DefaultTenantClient.PostAsJsonAsync("api/tenants/create", new
        {
            Name = name, RequestUrlPrefix = name, DatabaseProvider = "Sqlite", RecipeName = "Blank",
        }, TestContext.Current.CancellationToken);
        created.EnsureSuccessStatusCode();
        Assert.True(SiteContext.ShellHost.TryGetSettings(name, out var settings));
        var credentials = provision ? RemoteManagementClientCredentials.Generate() : null;
        var scope = await SiteContext.ShellHost.GetScopeAsync(settings);
        SiteContext.HttpContextAccessor.HttpContext = scope.ShellContext.CreateHttpContext();
        try
        {
            await scope.UsingAsync(async childScope =>
            {
                var service = new AutoSetupService(SiteContext.ShellHost, SiteContext.ShellSettingsManager,
                    childScope.ServiceProvider.GetRequiredService<ISetupService>(), NullLogger<AutoSetupService>.Instance);
                var (context, success) = await service.SetupTenantAsync(new TenantSetupOptions
                {
                    ShellName = name, SiteName = "Unattended test", AdminUsername = "admin", AdminEmail = "admin@example.com",
                    AdminPassword = "Password01_", DatabaseProvider = "Sqlite", RecipeName = "Blank", SiteTimeZone = "UTC",
                    RemoteManagementClientId = credentials?.ClientId, RemoteManagementClientSecret = credentials?.ClientSecret,
                }, settings);
                Assert.True(success, string.Join("; ", context.Errors.Values));
            });

            var verification = await SiteContext.ShellHost.GetScopeAsync(name);
            await verification.UsingAsync(async childScope =>
            {
                var features = await childScope.ServiceProvider.GetRequiredService<IShellFeaturesManager>().GetEnabledFeaturesAsync();
                Assert.Equal(provision, features.Any(feature => feature.Id == "OrchardCore.RemoteManagement.Cli"));
                if (provision)
                {
                    var manager = childScope.ServiceProvider.GetRequiredService<IOpenIdApplicationManager>();
                    Assert.NotNull(await manager.FindByClientIdAsync(credentials.ClientId));
                }
            });
        }
        finally
        {
            SiteContext.HttpContextAccessor.HttpContext = null;
        }
    }

    [Theory]
    [InlineData("Blank")]
    [InlineData("SaaS")]
    public async Task Provision_AfterSetup_AuthenticatesApplicationAndPreservesAdministrator(string recipe)
    {
        using var site = new SiteContext { RecipeName = recipe };
        await site.InitializeAsync();
        var credentials = RemoteManagementClientCredentials.Generate();
        Assert.True(SiteContext.ShellHost.TryGetSettings(site.TenantName, out var settings));
        if (recipe == "Blank")
        {
            await site.UsingTenantScopeAsync(async scope =>
            {
                Assert.DoesNotContain(await scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>().GetEnabledFeaturesAsync(),
                    feature => feature.Id == "OrchardCore.RemoteManagement.Cli");
            });
        }

        Assert.True(await RemoteManagementProvisioning.ConfigureAsync(SiteContext.ShellHost, settings, credentials));
        await site.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIdApplicationManager>();
            var application = await manager.FindByClientIdAsync(credentials.ClientId);
            Assert.NotNull(application);
            Assert.True(await manager.ValidateClientSecretAsync(application, credentials.ClientSecret));
            Assert.False(await manager.ValidateClientSecretAsync(application, "incorrect-secret"));
            var users = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
            Assert.True(await users.CheckPasswordAsync(await users.FindByNameAsync("admin"), "Password01_"));
        });

        using var tokenResponse = await site.Client.PostAsync("connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = credentials.ClientId,
            ["client_secret"] = credentials.ClientSecret,
            ["scope"] = RemoteManagementConstants.ManagementScope,
        }), TestContext.Current.CancellationToken);
        var tokenPayload = await tokenResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(tokenResponse.IsSuccessStatusCode, tokenPayload);
        using var token = JsonDocument.Parse(tokenPayload);
        site.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.RootElement.GetProperty("access_token").GetString());
        using var manifest = await site.Client.GetAsync("api/management/manifest", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, manifest.StatusCode);
        using var features = await site.Client.GetAsync("api/features", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, features.StatusCode);

        using var openApi = await site.Client.GetAsync("openapi/v1.json", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, openApi.StatusCode);
        using var document = JsonDocument.Parse(await openApi.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var scheme = document.RootElement.GetProperty("components").GetProperty("securitySchemes").GetProperty("Bearer");
        Assert.Equal("http", scheme.GetProperty("type").GetString());
        Assert.Equal("bearer", scheme.GetProperty("scheme").GetString());
        var manifestOperation = document.RootElement.GetProperty("paths").GetProperty("/api/management/manifest").GetProperty("get");
        Assert.True(manifestOperation.GetProperty("security")[0].TryGetProperty("Bearer", out _));

        using var me = await site.Client.GetAsync("api/management/me", TestContext.Current.CancellationToken);
        var mePayload = await me.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(me.IsSuccessStatusCode, mePayload);
        using var identity = JsonDocument.Parse(mePayload);
        Assert.Equal(site.TenantName, identity.RootElement.GetProperty("tenant").GetString());
        Assert.Equal("client", identity.RootElement.GetProperty("type").GetString());
        Assert.Equal(credentials.ClientId, identity.RootElement.GetProperty("clientId").GetString());
        Assert.Equal(credentials.ClientId, identity.RootElement.GetProperty("subject").GetString());
        Assert.Contains(identity.RootElement.GetProperty("scopes").EnumerateArray(), scope => scope.GetString() == RemoteManagementConstants.ManagementScope);
        Assert.Contains(identity.RootElement.GetProperty("permissions").EnumerateArray(), permission => permission.GetString() == RemoteManagementPermissions.AccessRemoteManagement.Name);
    }

    [Fact]
    public async Task EnableAndDisableRemoteManagement_RunningTenant_AppliesFeaturesAndRecordsAuditEvents()
    {
        using var site = new SiteContext { RecipeName = "Blank" };
        await site.InitializeAsync();
        var defaultSettings = new ShellSettings { Name = ShellSettings.DefaultShellName, State = TenantState.Running };
        var authorization = new Mock<IAuthorizationService>();
        authorization
            .Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
        var auditTrail = new Mock<IAuditTrailManager>();
        await using var services = new ServiceCollection().AddSingleton(auditTrail.Object).BuildServiceProvider();
        var localizer = Mock.Of<IStringLocalizer<TenantApiController>>();
        var logger = Mock.Of<ILogger<TenantApiController>>();

        var enabled = await TenantManagementEndpoints.EnableRemoteManagementAsync(new DefaultHttpContext { RequestServices = services }, site.TenantName,
            SiteContext.ShellHost, defaultSettings, authorization.Object, localizer, logger, provisionClient: true);

        var credentials = Assert.IsType<Ok<TenantManagementEndpoints.TenantOperationResponse>>(enabled).Value.ClientCredentials;
        Assert.NotNull(credentials);
        auditTrail.Verify(manager => manager.RecordEventAsync(It.Is<AuditTrailContext<TenantRemoteManagementAuditEvent>>(context =>
            context.Name == TenantRemoteManagementAuditTrailEventConfiguration.Enabled &&
            context.AuditTrailEventItem.TenantName == site.TenantName &&
            context.AuditTrailEventItem.ClientId == credentials.ClientId)), Times.Once);

        using var tokenResponse = await site.Client.PostAsync("connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = credentials.ClientId,
            ["client_secret"] = credentials.ClientSecret,
            ["scope"] = RemoteManagementConstants.ManagementScope,
        }), TestContext.Current.CancellationToken);
        var tokenPayload = await tokenResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(tokenResponse.IsSuccessStatusCode, tokenPayload);
        using var token = JsonDocument.Parse(tokenPayload);
        site.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.RootElement.GetProperty("access_token").GetString());
        using var before = await site.Client.GetAsync("api/management/manifest", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, before.StatusCode);

        var disabled = await TenantManagementEndpoints.DisableRemoteManagementAsync(new DefaultHttpContext { RequestServices = services }, site.TenantName,
            SiteContext.ShellHost, defaultSettings, authorization.Object, localizer, logger);

        Assert.Equal("disable-remote-management", Assert.IsType<Ok<TenantManagementEndpoints.TenantOperationResponse>>(disabled).Value.Action);
        auditTrail.Verify(manager => manager.RecordEventAsync(It.Is<AuditTrailContext<TenantRemoteManagementAuditEvent>>(context =>
            context.Name == TenantRemoteManagementAuditTrailEventConfiguration.Disabled &&
            context.AuditTrailEventItem.TenantName == site.TenantName)), Times.Once);
        await site.UsingTenantScopeAsync(async scope =>
        {
            var features = await scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>().GetEnabledFeaturesAsync();
            Assert.DoesNotContain(features, feature => feature.Id.Contains("RemoteManagement", StringComparison.Ordinal));
            Assert.Contains(features, feature => feature.Id == "OrchardCore.OpenId.Server");

            // Disabling keeps the applications, as other clients of the server can use them.
            Assert.NotNull(await scope.ServiceProvider.GetRequiredService<IOpenIdApplicationManager>().FindByClientIdAsync(credentials.ClientId));
        });
        using var after = await site.Client.GetAsync("api/management/manifest", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, after.StatusCode);

        // Repeating the operation is harmless.
        Assert.IsType<Ok<TenantManagementEndpoints.TenantOperationResponse>>(await TenantManagementEndpoints.DisableRemoteManagementAsync(
            new DefaultHttpContext { RequestServices = services }, site.TenantName, SiteContext.ShellHost, defaultSettings, authorization.Object, localizer, logger));
    }
}

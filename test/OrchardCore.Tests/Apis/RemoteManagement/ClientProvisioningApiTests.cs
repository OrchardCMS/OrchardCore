using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using OrchardCore.BackgroundTasks;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.AutoSetup.Options;
using OrchardCore.AutoSetup.Services;
using OrchardCore.Setup.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.RemoteManagement;
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
    }
}

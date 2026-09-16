using System.Security.Claims;
using Fluid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Abstractions.Setup;
using OrchardCore.Data;
using OrchardCore.Email;
using OrchardCore.Modules;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Recipes.Models;
using OrchardCore.RemoteManagement;
using OrchardCore.Setup.Services;
using OrchardCore.Tenants.Controllers;
using OrchardCore.Tenants.Endpoints.Management;
using OrchardCore.Tenants.Models;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Tenants;

public class TenantManagementEndpointsTests
{
    [Fact]
    public async Task TenantCapabilityProvider_AdvertisesTenantManagement()
    {
        var capabilities = await new TenantRemoteManagementCapabilityProvider().GetCapabilitiesAsync();
        var capability = Assert.Single(capabilities);

        Assert.Equal(TenantManagementApiEndpointConventions.CapabilityName, capability.Id);
        Assert.Equal("Tenants", capability.DisplayName);
        Assert.Equal(
            $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
            capability.Version);
    }

    [Fact]
    public void CreateTenantResponse_RedactsSecretsAndBuildsUrls()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("admin.example.com");
        httpContext.Features.Set(new ShellContextFeature
        {
            OriginalPathBase = "/root",
        });

        var shellSettings = new ShellSettings
        {
            Name = "TenantA",
            RequestUrlHost = "tenant-a.example.com,{host}",
            RequestUrlPrefix = "tenant-a",
            State = TenantState.Uninitialized,
        };

        shellSettings["Category"] = "Partners";
        shellSettings["Description"] = "Tenant description";
        shellSettings["DatabaseProvider"] = "SqlConnection";
        shellSettings["ConnectionString"] = "Server=db;User Id=orchard;Password=super-secret;";
        shellSettings["TablePrefix"] = "TenantA";
        shellSettings["Schema"] = "dbo";
        shellSettings["RecipeName"] = "Blog";
        shellSettings["FeatureProfile"] = "ProfileA, ProfileB";
        shellSettings["Secret"] = "tenant-secret";

        var clock = new Mock<IClock>();
        clock.SetupGet(c => c.UtcNow).Returns(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var response = TenantManagementEndpoints.CreateTenantResponse(
            httpContext,
            shellSettings,
            new EphemeralDataProtectionProvider(),
            clock.Object);

        Assert.Equal("TenantA", response.Name);
        Assert.Equal(["ProfileA", "ProfileB"], response.FeatureProfiles);
        Assert.Equal(["https://tenant-a.example.com/root/tenant-a"], response.Urls);
        Assert.Equal("https://tenant-a.example.com/root/tenant-a", response.PrimaryUrl);
        Assert.StartsWith("https://tenant-a.example.com/root/tenant-a?token=", response.SetupUrl, StringComparison.Ordinal);
        Assert.DoesNotContain("super-secret", response.ConnectionString, StringComparison.Ordinal);
        Assert.True(response.CanDelete);
        Assert.False(response.CanStart);
        Assert.False(response.CanStop);
    }

    [Fact]
    public void MatchesCreateRequest_IdenticalSettings_ReturnsTrue()
    {
        var settings = new ShellSettings
        {
            Name = "TenantA",
            RequestUrlPrefix = "tenant-a",
        };
        settings["DatabaseProvider"] = "Sqlite";
        settings["TablePrefix"] = "TenantA";
        settings["RecipeName"] = "SaaS";
        var model = new TenantApiModel
        {
            Name = "TenantA",
            RequestUrlPrefix = "tenant-a",
            DatabaseProvider = "Sqlite",
            TablePrefix = "TenantA",
            RecipeName = "SaaS",
        };

        Assert.True(TenantManagementEndpoints.MatchesCreateRequest(settings, model));

        model.Description = "Different";
        Assert.False(TenantManagementEndpoints.MatchesCreateRequest(settings, model));
    }

    [Fact]
    public async Task SetupAsync_UninitializedTenant_ExecutesSetupWithoutReturningPassword()
    {
        var tenantSettings = new ShellSettings
        {
            Name = "TenantA",
            State = TenantState.Uninitialized,
        };
        tenantSettings["RecipeName"] = "SaaS";

        var shellHost = new Mock<IShellHost>();
        shellHost
            .Setup(host => host.TryGetSettings("TenantA", out tenantSettings))
            .Returns(true);

        var shellSettingsManager = new Mock<IShellSettingsManager>();
        shellSettingsManager
            .Setup(manager => manager.CreateDefaultSettings())
            .Returns(new ShellSettings());

        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        SetupContext capturedContext = null;
        var setupService = new Mock<ISetupService>();
        setupService
            .Setup(service => service.GetSetupRecipesAsync())
            .ReturnsAsync([new RecipeDescriptor { Name = "SaaS" }]);
        setupService
            .Setup(service => service.SetupAsync(It.IsAny<SetupContext>()))
            .Callback<SetupContext>(context =>
            {
                capturedContext = context;
                tenantSettings.State = TenantState.Running;
            })
            .ReturnsAsync("execution-id");

        var emailValidator = new Mock<IEmailAddressValidator>();
        emailValidator.Setup(validator => validator.Validate("admin@example.com")).Returns(true);
        var localizer = Mock.Of<IStringLocalizer<TenantApiController>>();
        var patternResolver = new TenantDatabasePatternResolver(
            new FluidParser(),
            Options.Create(new global::OrchardCore.Tenants.TenantsOptions()),
            Mock.Of<IStringLocalizer<TenantDatabasePatternResolver>>());
        var distributedLock = new Mock<IDistributedLock>();
        distributedLock
            .Setup(@lock => @lock.TryAcquireLockAsync(
                "TENANT_SETUP_TENANTA",
                TimeSpan.FromSeconds(3),
                TimeSpan.FromHours(1)))
            .ReturnsAsync((Mock.Of<ILocker>(), true));
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("cms.example.com");

        var result = await TenantManagementEndpoints.SetupAsync(
            httpContext,
            "TenantA",
            new TenantManagementEndpoints.TenantSetupRequest
            {
                SiteName = "Tenant A",
                UserName = "admin",
                Email = "admin@example.com",
                Password = "Secret1!",
                SiteTimeZone = "Etc/UTC",
                DatabaseProvider = "sqlite",
            },
            shellHost.Object,
            new ShellSettings { Name = ShellSettings.DefaultShellName },
            shellSettingsManager.Object,
            authorizationService.Object,
            new EphemeralDataProtectionProvider(),
            Mock.Of<IClock>(),
            setupService.Object,
            emailValidator.Object,
            Options.Create(new IdentityOptions()),
            [new DatabaseProvider { Name = "Sqlite", Value = "Sqlite" }],
            patternResolver,
            distributedLock.Object,
            localizer,
            Mock.Of<ILogger<TenantApiController>>());

        Assert.Equal(StatusCodes.Status200OK, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        Assert.NotNull(capturedContext);
        Assert.Equal("Secret1!", capturedContext.Properties[SetupConstants.AdminPassword]);
        Assert.Equal("Sqlite", capturedContext.Properties[SetupConstants.DatabaseProvider]);
        Assert.DoesNotContain("Secret1!", result.ToString(), StringComparison.Ordinal);
        distributedLock.Verify(@lock => @lock.TryAcquireLockAsync(
            "TENANT_SETUP_TENANTA",
            TimeSpan.FromSeconds(3),
            TimeSpan.FromHours(1)), Times.Once);
    }
}

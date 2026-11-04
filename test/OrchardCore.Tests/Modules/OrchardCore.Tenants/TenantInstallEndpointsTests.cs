using System.Security.Claims;
using Fluid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Abstractions.Setup;
using OrchardCore.Data;
using OrchardCore.Email;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Recipes.Models;
using OrchardCore.Setup.Services;
using OrchardCore.Tenants.Controllers;
using OrchardCore.Tenants.Endpoints.Management;
using OrchardCore.Tenants.Models;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Tenants;

public class TenantInstallEndpointsTests
{
    [Fact]
    public async Task Install_NewTenant_RunsSetupAndReturnsCreatedTenant()
    {
        var fixture = new Fixture();
        var result = await fixture.InstallAsync();
        Assert.Equal(201, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        var tenant = Assert.IsType<TenantManagementEndpoints.TenantResponse>(Assert.IsAssignableFrom<IValueHttpResult>(result).Value);
        Assert.Equal("Running", tenant.State);
        Assert.Equal("https://cms.example.com/blog", tenant.PrimaryUrl);
        Assert.Null(tenant.SetupUrl);
        Assert.Equal("Secret1!", fixture.SetupContext.Properties[SetupConstants.AdminPassword]);
        Assert.Equal("Tenant A", fixture.SetupContext.Properties[SetupConstants.SiteName]);
        Assert.Equal("Etc/UTC", fixture.SetupContext.Properties[SetupConstants.SiteTimeZone]);
        Assert.Equal("SaaS", fixture.SetupContext.Recipe.Name);
        Assert.Equal("Sqlite", fixture.SetupContext.Properties[SetupConstants.DatabaseProvider]);
        fixture.DistributedLock.Verify(x => x.TryAcquireLockAsync("TENANT_SETUP_TENANTA", TimeSpan.FromSeconds(3), TimeSpan.FromHours(1)), Times.Once);
    }

    [Fact]
    public async Task Install_HostDatabasePreset_TakesPrecedenceOverDefault()
    {
        var fixture = new Fixture { HostDatabaseProvider = "SqlConnection" };
        var result = await fixture.InstallAsync();
        Assert.Equal(201, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        Assert.Equal("SqlConnection", fixture.Tenant["DatabaseProvider"]);
        Assert.Equal("SqlConnection", fixture.SetupContext.Properties[SetupConstants.DatabaseProvider]);
        Assert.Equal("Server=localhost;Database=TenantA;Integrated Security=true",
            fixture.SetupContext.Properties[SetupConstants.DatabaseConnectionString]);
    }

    [Theory]
    [InlineData("{}", "Sqlite")]
    [InlineData("{\"databaseProvider\":\"Postgres\"}", "Postgres")]
    public void InstallRequest_DatabaseProvider_UsesDefaultOnlyWhenOmitted(string json, string expected)
    {
        var request = System.Text.Json.JsonSerializer.Deserialize<TenantManagementEndpoints.TenantInstallRequest>(json,
            System.Text.Json.JsonSerializerOptions.Web);
        Assert.Equal(expected, request.ToCreateRequest("TenantA").DatabaseProvider);
        Assert.Equal(expected, request.ToSetupRequest().DatabaseProvider);
    }

    [Theory]
    [InlineData(TenantState.Uninitialized)]
    [InlineData(TenantState.Running)]
    [InlineData(TenantState.Disabled)]
    public async Task Install_ExistingTenant_RejectsWithoutChangingIt(TenantState state)
    {
        var fixture = new Fixture { Tenant = new ShellSettings { Name = "TenantA", State = state } };
        var result = await fixture.InstallAsync();
        Assert.Equal(409, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        Assert.Equal(state, fixture.Tenant.State);
        Assert.Null(fixture.SetupContext);
        fixture.Host.Verify(x => x.UpdateShellSettingsAsync(It.IsAny<ShellSettings>()), Times.Never);
    }

    [Theory]
    [InlineData(false, true, 403)]
    [InlineData(true, false, 403)]
    public async Task Install_Unauthorized_DoesNotCreateTenant(bool authorized, bool defaultTenant, int status)
    {
        var fixture = new Fixture { Authorized = authorized, DefaultTenant = defaultTenant };
        Assert.Equal(status, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await fixture.InstallAsync()).StatusCode);
        Assert.Null(fixture.Tenant);
        Assert.Null(fixture.SetupContext);
        fixture.DistributedLock.Verify(x => x.TryAcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task Install_ConcurrentSetup_RejectsWithoutCreatingTenant()
    {
        var fixture = new Fixture { LockAvailable = false };
        Assert.Equal(409, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await fixture.InstallAsync()).StatusCode);
        Assert.Null(fixture.Tenant);
    }

    [Fact]
    public async Task Install_InvalidCreation_DoesNotRunSetup()
    {
        var fixture = new Fixture();
        fixture.Validator.Setup(x => x.ValidateAsync(It.IsAny<TenantModelBase>()))
            .ReturnsAsync([new ModelError("Name", "Invalid tenant name.")]);
        Assert.Equal(400, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await fixture.InstallAsync()).StatusCode);
        Assert.Null(fixture.Tenant);
        Assert.Null(fixture.SetupContext);
    }

    [Theory]
    [InlineData(false, 400)]
    [InlineData(true, 500)]
    public async Task Install_SetupFailure_PreservesTenantAndReportsRecovery(bool throws, int status)
    {
        var fixture = new Fixture { FailSetup = true, ThrowDuringSetup = throws };
        var result = await fixture.InstallAsync();
        Assert.Equal(status, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        Assert.Equal(TenantState.Uninitialized, fixture.Tenant.State);
        var problem = throws ? Assert.IsType<ProblemHttpResult>(result).ProblemDetails : Assert.IsType<ValidationProblem>(result).ProblemDetails;
        Assert.Equal("setup", problem.Extensions["stage"]);
        Assert.Equal("TenantA", problem.Extensions["tenantName"]);
        Assert.Contains("tenant was created", problem.Detail);
        Assert.DoesNotContain("Secret1!", problem.Detail);
        Assert.Equal(409, Assert.IsAssignableFrom<IStatusCodeHttpResult>(await fixture.InstallAsync()).StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Aa1!")]
    [InlineData("lowercase1!")]
    [InlineData("UPPERCASE1!")]
    [InlineData("NoDigitsHere!")]
    [InlineData("NoSymbols123")]
    [InlineData("Élowercase1!")]
    public async Task Install_InvalidPassword_DoesNotCreateTenantOrRunSetup(string password)
    {
        var fixture = new Fixture { Password = password };
        var result = Assert.IsType<ValidationProblem>(await fixture.InstallAsync());
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Password", result.ProblemDetails.Errors.Keys);
        Assert.Null(fixture.Tenant);
        Assert.Null(fixture.SetupContext);
        fixture.Host.Verify(x => x.UpdateShellSettingsAsync(It.IsAny<ShellSettings>()), Times.Never);
    }

    [Theory]
    [InlineData(20, 1)]
    [InlineData(6, 10)]
    public async Task Install_CustomPasswordRequirements_RejectsBeforeCreation(int length, int unique)
    {
        var fixture = new Fixture();
        fixture.IdentityOptions.Password.RequiredLength = length;
        fixture.IdentityOptions.Password.RequiredUniqueChars = unique;
        Assert.IsType<ValidationProblem>(await fixture.InstallAsync());
        Assert.Null(fixture.Tenant);
        Assert.Null(fixture.SetupContext);
    }

    private sealed class Fixture
    {
        public Mock<IShellHost> Host { get; } = new();
        public Mock<ITenantValidator> Validator { get; } = new();
        public Mock<IDistributedLock> DistributedLock { get; } = new();
        public ShellSettings Tenant { get; set; }
        public SetupContext SetupContext { get; private set; }
        public string Password { get; init; } = "Secret1!";
        public IdentityOptions IdentityOptions { get; } = new();
        public bool Authorized { get; init; } = true;
        public bool DefaultTenant { get; init; } = true;
        public bool LockAvailable { get; init; } = true;
        public string HostDatabaseProvider { get; init; }
        public bool FailSetup { get; init; }
        public bool ThrowDuringSetup { get; init; }

        public Fixture()
        {
            Host.Setup(x => x.TryGetSettings("TenantA", out It.Ref<ShellSettings>.IsAny))
                .Returns((string name, out ShellSettings settings) => { settings = Tenant; return settings is not null; });
            Host.Setup(x => x.UpdateShellSettingsAsync(It.IsAny<ShellSettings>()))
                .Callback<ShellSettings>(settings => Tenant = new ShellSettings(settings)).Returns(Task.CompletedTask);
            Validator.Setup(x => x.ValidateAsync(It.IsAny<TenantModelBase>())).ReturnsAsync([]);
            DistributedLock.Setup(x => x.TryAcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(() => (Mock.Of<ILocker>(), LockAvailable));
        }

        public async Task<IResult> InstallAsync()
        {
            var authorization = new Mock<IAuthorizationService>();
            authorization.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(Authorized ? AuthorizationResult.Success() : AuthorizationResult.Failed());
            var manager = new Mock<IShellSettingsManager>();
            manager.Setup(x => x.CreateDefaultSettings()).Returns(() =>
            {
                var settings = new ShellSettings();
                if (HostDatabaseProvider is not null)
                {
                    settings["DatabaseProvider"] = HostDatabaseProvider;
                    settings["ConnectionString"] = "Server=localhost;Database=TenantA;Integrated Security=true";
                }
                return settings;
            });
            var setup = new Mock<ISetupService>();
            setup.Setup(x => x.GetSetupRecipesAsync()).ReturnsAsync([new RecipeDescriptor { Name = "SaaS" }]);
            setup.Setup(x => x.SetupAsync(It.IsAny<SetupContext>())).Returns<SetupContext>(context =>
            {
                SetupContext = context;
                if (ThrowDuringSetup)
                {
                    throw new InvalidOperationException("Sensitive exception details.");
                }
                if (FailSetup)
                {
                    context.Errors["Password"] = "Password does not meet requirements.";
                }
                else
                {
                    Tenant.State = TenantState.Running;
                }
                return Task.FromResult("execution-id");
            });
            var email = new Mock<IEmailAddressValidator>();
            email.Setup(x => x.Validate("admin@example.com")).Returns(true);
            var localizer = new Mock<IStringLocalizer<TenantApiController>>();
            localizer.Setup(x => x[It.IsAny<string>()]).Returns((string text) => new LocalizedString(text, text));
            localizer.Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
                .Returns((string text, object[] args) => new LocalizedString(text, string.Format(text, args)));
            using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
            var context = new DefaultHttpContext { RequestServices = services };
            context.Request.Scheme = "https";
            context.Request.Host = new HostString("cms.example.com");
            return await TenantManagementEndpoints.InstallAsync(context, "TenantA", new TenantManagementEndpoints.TenantInstallRequest
            {
                RequestUrlPrefix = "blog", RecipeName = "SaaS",
                SiteName = "Tenant A", UserName = "admin", Email = "admin@example.com", Password = Password, SiteTimeZone = "Etc/UTC",
            }, Host.Object, manager.Object, new EphemeralDataProtectionProvider(), Mock.Of<IClock>(),
                [new DatabaseProvider { Name = "Sqlite", Value = "Sqlite" }, new DatabaseProvider { Name = "SQL Server", Value = "SqlConnection", HasConnectionString = true }], Validator.Object,
                new TenantDatabasePatternResolver(new FluidParser(), Options.Create(new global::OrchardCore.Tenants.TenantsOptions()), Mock.Of<IStringLocalizer<TenantDatabasePatternResolver>>()),
                localizer.Object, Mock.Of<ILogger<TenantApiController>>(), setup.Object, email.Object, Options.Create(IdentityOptions),
                new ShellSettings { Name = DefaultTenant ? ShellSettings.DefaultShellName : "Other" }, authorization.Object, DistributedLock.Object);
        }
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;
using OrchardCore.Setup.Services;
using OrchardCore.Tenants.Controllers;
using OrchardCore.Tenants.Endpoints.Management;
using OrchardCore.Tests.Apis.Context;
using YesSql;

namespace OrchardCore.Tests.Modules.OrchardCore.Tenants;

public class TenantSetupDiagnosticsTests
{
    [Fact]
    public async Task SetupOptions_ManageTenants_ReturnsVisibleRecipesAndProviders()
    {
        var setup = new Mock<ISetupService>();
        setup.Setup(value => value.GetSetupRecipesAsync()).ReturnsAsync(
        [
            new RecipeDescriptor { Name = "Saas", DisplayName = "Software as a Service", Tags = ["default"], Categories = ["default"] },
            new RecipeDescriptor { Name = "Blog", DisplayName = "Blog", Description = "A blog" },
            new RecipeDescriptor { Name = "Tool", DisplayName = "Tool", Tags = ["HIDDEN"] },
        ]);
        DatabaseProvider[] providers =
        [
            new DatabaseProvider { Name = "Sqlite", Value = "Sqlite", HasTablePrefix = true, IsDefault = true },
            new DatabaseProvider { Name = "MySql", Value = "MySql", HasConnectionString = true, SampleConnectionString = "Server=localhost" },
        ];

        var result = await TenantManagementEndpoints.GetSetupOptionsAsync(Context(), Authorization(true), new ShellSettings { Name = ShellSettings.DefaultShellName }, setup.Object, providers, Localizer());

        var options = Assert.IsType<Ok<TenantManagementEndpoints.TenantSetupOptionsResponse>>(result).Value;
        Assert.Equal(["Blog", "Saas"], options.Recipes.Select(recipe => recipe.Name));
        Assert.Equal("A blog", options.Recipes[0].Description);
        Assert.Equal(["MySql", "Sqlite"], options.DatabaseProviders.Select(provider => provider.Value));
        Assert.True(options.DatabaseProviders[0].HasConnectionString);
        Assert.True(options.DatabaseProviders[1].IsDefault);
    }

    [Theory]
    [InlineData("Default", false)]
    [InlineData("Other", true)]
    public async Task Diagnostics_NotAuthorizedOrNotDefaultTenant_ReturnsForbidden(string tenant, bool authorized)
    {
        var setup = new Mock<ISetupService>(MockBehavior.Strict);
        var failures = new FakeFailureStore();
        var host = new Mock<IShellHost>(MockBehavior.Strict);
        var settings = new ShellSettings { Name = tenant };

        var options = await TenantManagementEndpoints.GetSetupOptionsAsync(Context(), Authorization(authorized), settings, setup.Object, [], Localizer());
        var status = await TenantManagementEndpoints.GetSetupStatusAsync(Context(), "Blog", Authorization(authorized), settings, host.Object, failures, Localizer());

        Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(options).StatusCode);
        Assert.Equal(403, Assert.IsAssignableFrom<IStatusCodeHttpResult>(status).StatusCode);
    }

    [Fact]
    public async Task SetupStatus_UnknownTenant_ReturnsNotFound()
    {
        var host = new Mock<IShellHost>();
        ShellSettings missing = null;
        host.Setup(value => value.TryGetSettings("Blog", out missing)).Returns(false);

        var result = await TenantManagementEndpoints.GetSetupStatusAsync(Context(), "Blog", Authorization(true),
            new ShellSettings { Name = ShellSettings.DefaultShellName }, host.Object, new FakeFailureStore(), Localizer());

        Assert.Equal(404, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
    }

    [Theory]
    [InlineData(TenantState.Uninitialized, true)]
    [InlineData(TenantState.Running, false)]
    public async Task SetupStatus_TenantWithFailure_ReturnsItOnlyWhileUninitialized(TenantState state, bool expected)
    {
        var tenant = new ShellSettings { Name = "Blog", State = state };
        var host = new Mock<IShellHost>();
        host.Setup(value => value.TryGetSettings("blog", out tenant)).Returns(true);
        var failures = new FakeFailureStore { Recorded = { ["Blog"] = "The recipe failed." } };

        var result = await TenantManagementEndpoints.GetSetupStatusAsync(Context(), "blog", Authorization(true),
            new ShellSettings { Name = ShellSettings.DefaultShellName }, host.Object, failures, Localizer());

        var status = Assert.IsType<Ok<TenantManagementEndpoints.TenantSetupStatusResponse>>(result).Value;
        Assert.Equal("Blog", status.TenantName);
        Assert.Equal(state.ToString(), status.State);
        Assert.Equal(expected ? "The recipe failed." : null, status.LastFailure?.Message);
    }

    [Fact]
    public async Task Setup_ReportsErrors_RecordsThemAndKeepsTheResult()
    {
        var context = SetupContext();
        var setup = new Mock<ISetupService>();
        setup.Setup(value => value.SetupAsync(context)).Callback(() =>
        {
            context.Errors["Recipe"] = "First.";
            context.Errors["Database"] = "Second.";
        }).ReturnsAsync("execution");
        var failures = new FakeFailureStore();

        Assert.Equal("execution", await new FailureRecordingSetupService(setup.Object, failures, new FailureLocalizer()).SetupAsync(context));

        Assert.Equal("First. Second.", failures.Recorded["Blog"]);
        Assert.Empty(failures.Cleared);
    }

    [Fact]
    public async Task Setup_Throws_RecordsAGenericMessageAndRethrows()
    {
        var context = SetupContext();
        var setup = new Mock<ISetupService>();
        setup.Setup(value => value.SetupAsync(context)).ThrowsAsync(new InvalidOperationException("Password=private"));
        var failures = new FakeFailureStore();

        await Assert.ThrowsAsync<InvalidOperationException>(() => new FailureRecordingSetupService(setup.Object, failures, new FailureLocalizer()).SetupAsync(context));

        Assert.DoesNotContain("private", failures.Recorded["Blog"]);
    }

    [Fact]
    public async Task Setup_Succeeds_ClearsThePreviousFailure()
    {
        var context = SetupContext();
        var setup = new Mock<ISetupService>();
        setup.Setup(value => value.SetupAsync(context)).ReturnsAsync("execution");
        var failures = new FakeFailureStore();

        await new FailureRecordingSetupService(setup.Object, failures, new FailureLocalizer()).SetupAsync(context);

        Assert.Equal(["Blog"], failures.Cleared);
        Assert.Empty(failures.Recorded);
    }

    [Fact]
    public async Task Store_FreshScopes_KeepsTheLastFailurePerTenantUntilCleared()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        var now = new DateTime(2026, 11, 2, 8, 15, 4, DateTimeKind.Utc);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var store = Store(scope.ServiceProvider, () => now);
            Assert.Null(await store.FindAsync("Blog"));
            await store.RecordAsync(ShellSettings.DefaultShellName, "Older.");
            await store.RecordAsync(ShellSettings.DefaultShellName, "Newer.");
            await store.ClearAsync("Unknown");
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var store = Store(scope.ServiceProvider, () => now);
            var failure = await store.FindAsync(ShellSettings.DefaultShellName);
            Assert.Equal("Newer.", failure.Message);
            Assert.Equal(now, failure.OccurredUtc);

            await store.ClearAsync(ShellSettings.DefaultShellName);
            Assert.Null(await store.FindAsync(ShellSettings.DefaultShellName));
        });
    }

    [Fact]
    public async Task Store_TenantWasDeleted_RemovesItsFailureWithTheNextOne()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var store = Store(scope.ServiceProvider, () => DateTime.UtcNow);
            await store.RecordAsync("DeletedTenant", "Failed.");
            Assert.NotNull(await store.FindAsync("DeletedTenant"));

            await store.RecordAsync(ShellSettings.DefaultShellName, "Failed.");

            Assert.Null(await store.FindAsync("DeletedTenant"));
            Assert.NotNull(await store.FindAsync(ShellSettings.DefaultShellName));
        });
    }

    private static TenantSetupFailureStore Store(IServiceProvider services, Func<DateTime> now)
    {
        var clock = new Mock<IClock>();
        clock.SetupGet(value => value.UtcNow).Returns(now);
        var shellHost = new Mock<IShellHost>();
        var settings = new ShellSettings { Name = ShellSettings.DefaultShellName };
        shellHost.Setup(value => value.TryGetSettings(ShellSettings.DefaultShellName, out settings)).Returns(true);

        return new TenantSetupFailureStore(services.GetRequiredService<IStore>(), shellHost.Object, clock.Object, NullLogger<TenantSetupFailureStore>.Instance);
    }

    private static SetupContext SetupContext() => new()
    {
        ShellSettings = new ShellSettings { Name = "Blog" },
        Errors = new Dictionary<string, string>(),
        Properties = new Dictionary<string, object>(),
    };

    private static DefaultHttpContext Context() => new()
    {
        RequestServices = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider(),
    };

    private static IAuthorizationService Authorization(bool authorized)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(authorized ? AuthorizationResult.Success() : AuthorizationResult.Failed());

        return authorization.Object;
    }

    private static IStringLocalizer<TenantApiController> Localizer()
    {
        var localizer = new Mock<IStringLocalizer<TenantApiController>>();
        localizer.Setup(value => value[It.IsAny<string>()]).Returns((string name) => new LocalizedString(name, name));
        localizer.Setup(value => value[It.IsAny<string>(), It.IsAny<object[]>()]).Returns((string name, object[] _) => new LocalizedString(name, name));

        return localizer.Object;
    }

    private sealed class FakeFailureStore : ITenantSetupFailureStore
    {
        public Dictionary<string, string> Recorded { get; } = [];
        public List<string> Cleared { get; } = [];

        public Task<TenantSetupFailure> FindAsync(string tenantName)
            => Task.FromResult(Recorded.TryGetValue(tenantName, out var message) ? new TenantSetupFailure { Message = message } : null);

        public Task RecordAsync(string tenantName, string message)
        {
            Recorded[tenantName] = message;
            return Task.CompletedTask;
        }

        public Task ClearAsync(string tenantName)
        {
            Cleared.Add(tenantName);
            return Task.CompletedTask;
        }
    }

    private sealed class FailureLocalizer : IStringLocalizer<FailureRecordingSetupService>
    {
        public LocalizedString this[string name] => new(name, name);
        public LocalizedString this[string name, params object[] arguments] => new(name, name);
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}

using OrchardCore.Abstractions.Setup;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Recipes.Models;
using OrchardCore.Setup.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Modules.OrchardCore.Setup.Tests;

public class SetupServiceTests
{
    [Fact]
    public async Task SetupAsync_ShouldResetStateToUninitialized_WhenDbValidationFails()
    {
        // Arrange
        var shellSettings = new ShellSettings { Name = "TestTenant" }.AsUninitialized();
        var shellHost = new Mock<IShellHost>();

        shellHost
            .Setup(x => x.ReloadShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        var dbConnectionValidator = new Mock<IDbConnectionValidator>();
        dbConnectionValidator
            .Setup(x => x.ValidateAsync(It.IsAny<DbConnectionValidatorContext>()))
            .ReturnsAsync(DbConnectionValidatorResult.InvalidConnection);

        var setupService = CreateSetupService(shellHost.Object, dbConnectionValidator.Object);

        var setupContext = CreateSetupContext(shellSettings);

        // Act
        var executionId = await setupService.SetupAsync(setupContext);

        // Assert - The state should be reset to Uninitialized after failure.
        Assert.True(setupContext.Errors.Count > 0);
        Assert.Equal(TenantState.Uninitialized, shellSettings.State);
    }

    [Fact]
    public async Task SetupAsync_ShouldCallReloadShellContextAsync_WhenSetupFails()
    {
        // Arrange
        var shellSettings = new ShellSettings { Name = "TestTenant" }.AsUninitialized();
        var shellHost = new Mock<IShellHost>();

        shellHost
            .Setup(x => x.ReloadShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        var dbConnectionValidator = new Mock<IDbConnectionValidator>();
        dbConnectionValidator
            .Setup(x => x.ValidateAsync(It.IsAny<DbConnectionValidatorContext>()))
            .ReturnsAsync(DbConnectionValidatorResult.InvalidConnection);

        var setupService = CreateSetupService(shellHost.Object, dbConnectionValidator.Object);

        var setupContext = CreateSetupContext(shellSettings);

        // Act
        await setupService.SetupAsync(setupContext);

        // Assert - ReloadShellContextAsync must be called to properly reset the shell.
        shellHost.Verify(x => x.ReloadShellContextAsync(
            It.Is<ShellSettings>(s => s.State == TenantState.Uninitialized),
            false), Times.Once);
    }

    [Fact]
    public async Task SetupAsync_ShouldResetToUninitialized_WhenExceptionIsThrown()
    {
        // Arrange
        var shellSettings = new ShellSettings { Name = "TestTenant" }.AsUninitialized();
        var shellHost = new Mock<IShellHost>();

        shellHost
            .Setup(x => x.ReloadShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        var dbConnectionValidator = new Mock<IDbConnectionValidator>();
        dbConnectionValidator
            .Setup(x => x.ValidateAsync(It.IsAny<DbConnectionValidatorContext>()))
            .ThrowsAsync(new InvalidOperationException("Simulated DB failure"));

        var setupService = CreateSetupService(shellHost.Object, dbConnectionValidator.Object);

        var setupContext = CreateSetupContext(shellSettings);

        // Act & Assert - Exception should propagate but state should be reset.
        await Assert.ThrowsAsync<InvalidOperationException>(() => setupService.SetupAsync(setupContext));
        Assert.Equal(TenantState.Uninitialized, shellSettings.State);
    }

    [Fact]
    public async Task SetupAsync_ShouldAllowRetryAfterPreviousFailure()
    {
        // Arrange - Simulate a tenant that previously had a failed setup and was reset.
        var shellSettings = new ShellSettings { Name = "TestTenant" }.AsUninitialized();
        var shellHost = new Mock<IShellHost>();

        shellHost
            .Setup(x => x.ReloadShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        var dbConnectionValidator = new Mock<IDbConnectionValidator>();
        dbConnectionValidator
            .Setup(x => x.ValidateAsync(It.IsAny<DbConnectionValidatorContext>()))
            .ReturnsAsync(DbConnectionValidatorResult.InvalidConnection);

        var setupService = CreateSetupService(shellHost.Object, dbConnectionValidator.Object);

        // First setup attempt fails.
        var setupContext1 = CreateSetupContext(shellSettings);
        await setupService.SetupAsync(setupContext1);
        Assert.True(setupContext1.Errors.Count > 0);
        Assert.Equal(TenantState.Uninitialized, shellSettings.State);

        // Second setup attempt should also be allowed since state is Uninitialized.
        var setupContext2 = CreateSetupContext(shellSettings);
        await setupService.SetupAsync(setupContext2);
        Assert.Equal(TenantState.Uninitialized, shellSettings.State);

        // Verify reload was called both times.
        shellHost.Verify(x => x.ReloadShellContextAsync(
            It.Is<ShellSettings>(s => s.State == TenantState.Uninitialized),
            false), Times.Exactly(2));
    }

    [Fact]
    public async Task SetupAsync_ShouldNotBlockOnDocumentTableFound_ForNonDefaultShell()
    {
        // Arrange - Simulate a non-default shell where Document table already exists
        // (from a previous failed setup attempt).
        var shellSettings = new ShellSettings { Name = "TestTenant" }.AsUninitialized();
        var shellHost = new Mock<IShellHost>();

        shellHost
            .Setup(x => x.ReloadShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        var dbConnectionValidator = new Mock<IDbConnectionValidator>();
        dbConnectionValidator
            .Setup(x => x.ValidateAsync(It.IsAny<DbConnectionValidatorContext>()))
            .ReturnsAsync(DbConnectionValidatorResult.DocumentTableFound);

        var setupService = CreateSetupService(shellHost.Object, dbConnectionValidator.Object);

        var setupContext = CreateSetupContext(shellSettings);

        // Act
        var executionId = await setupService.SetupAsync(setupContext);

        // Assert - The DocumentTableFound should NOT block setup for non-default shells.
        // The setup continues past validation (may fail later due to unrelated mock limitations).
        // Verify that the DB connection validator was called AND setup proceeded past it.
        dbConnectionValidator.Verify(x => x.ValidateAsync(It.IsAny<DbConnectionValidatorContext>()), Times.Once);

        // If DocumentTableFound had blocked, there would be exactly 1 error from validation.
        // Since it didn't block, any errors are from subsequent steps (shell context creation etc.).
        // The key assertion: the setup proceeded past the document table check.
        Assert.DoesNotContain(setupContext.Errors, kvp =>
            kvp.Value is not null && kvp.Value.ToString().Contains("already in use"));
    }

    [Fact]
    public async Task SetupAsync_ShouldBlockOnDocumentTableFound_ForDefaultShell()
    {
        // Arrange - The default shell should still block on DocumentTableFound.
        var shellSettings = new ShellSettings { Name = ShellSettings.DefaultShellName }.AsUninitialized();
        var shellHost = new Mock<IShellHost>();

        shellHost
            .Setup(x => x.ReloadShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        var dbConnectionValidator = new Mock<IDbConnectionValidator>();
        dbConnectionValidator
            .Setup(x => x.ValidateAsync(It.IsAny<DbConnectionValidatorContext>()))
            .ReturnsAsync(DbConnectionValidatorResult.DocumentTableFound);

        var setupService = CreateSetupService(shellHost.Object, dbConnectionValidator.Object);

        var setupContext = CreateSetupContext(shellSettings);

        // Act
        var executionId = await setupService.SetupAsync(setupContext);

        // Assert - Default shell should still get the error.
        Assert.True(setupContext.Errors.Count > 0);
    }

    [Fact]
    public async Task SetupAsync_ShouldPreventConcurrentSetup_ForSameTenant()
    {
        // Arrange
        var shellSettings = new ShellSettings { Name = "TestTenant" }.AsUninitialized();
        var shellHost = new Mock<IShellHost>();

        shellHost
            .Setup(x => x.ReloadShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        // Use a TaskCompletionSource to hold the first setup in progress.
        var tcs = new TaskCompletionSource<DbConnectionValidatorResult>();
        var dbConnectionValidator = new Mock<IDbConnectionValidator>();
        dbConnectionValidator
            .Setup(x => x.ValidateAsync(It.IsAny<DbConnectionValidatorContext>()))
            .Returns(tcs.Task);

        var setupTracker = CreateRealTracker();
        var setupService = CreateSetupService(shellHost.Object, dbConnectionValidator.Object, setupTracker);

        var setupContext1 = CreateSetupContext(shellSettings);
        var setupContext2 = CreateSetupContext(shellSettings);

        // Act - Start first setup (will block on DB validation).
        var firstSetupTask = setupService.SetupAsync(setupContext1);

        // Try second setup while first is in progress.
        var executionId2 = await setupService.SetupAsync(setupContext2);

        // Assert - Second setup should fail with "already in progress" error.
        Assert.True(setupContext2.Errors.Count > 0, "Second setup should have errors");
        Assert.Contains(setupContext2.Errors, e => e.Value.Contains("already in progress", StringComparison.OrdinalIgnoreCase));

        // Complete the first setup.
        tcs.SetResult(DbConnectionValidatorResult.InvalidConnection);
        await firstSetupTask;
    }

    [Fact]
    public async Task SetupAsync_ShouldReleaseTracker_AfterFailure()
    {
        // Arrange
        var shellSettings = new ShellSettings { Name = "TestTenant" }.AsUninitialized();
        var shellHost = new Mock<IShellHost>();

        shellHost
            .Setup(x => x.ReloadShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        var dbConnectionValidator = new Mock<IDbConnectionValidator>();
        dbConnectionValidator
            .Setup(x => x.ValidateAsync(It.IsAny<DbConnectionValidatorContext>()))
            .ReturnsAsync(DbConnectionValidatorResult.InvalidConnection);

        var setupTracker = CreateRealTracker();
        var setupService = CreateSetupService(shellHost.Object, dbConnectionValidator.Object, setupTracker);

        // Act - First setup fails.
        var setupContext1 = CreateSetupContext(shellSettings);
        await setupService.SetupAsync(setupContext1);
        Assert.True(setupContext1.Errors.Count > 0);

        // Assert - Tracker should be released, allowing retry.
        Assert.False(await setupTracker.IsSetupInProgressAsync(shellSettings));

        // Second setup should proceed (not blocked by tracker).
        var setupContext2 = CreateSetupContext(shellSettings);
        await setupService.SetupAsync(setupContext2);

        // Verify second attempt reached the DB validator.
        dbConnectionValidator.Verify(x => x.ValidateAsync(It.IsAny<DbConnectionValidatorContext>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SetupAsync_ShouldReleaseTracker_AfterException()
    {
        // Arrange
        var shellSettings = new ShellSettings { Name = "TestTenant" }.AsUninitialized();
        var shellHost = new Mock<IShellHost>();

        shellHost
            .Setup(x => x.ReloadShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        var dbConnectionValidator = new Mock<IDbConnectionValidator>();
        dbConnectionValidator
            .Setup(x => x.ValidateAsync(It.IsAny<DbConnectionValidatorContext>()))
            .ThrowsAsync(new InvalidOperationException("Simulated failure"));

        var setupTracker = CreateRealTracker();
        var setupService = CreateSetupService(shellHost.Object, dbConnectionValidator.Object, setupTracker);

        // Act - First setup throws.
        var setupContext1 = CreateSetupContext(shellSettings);
        await Assert.ThrowsAsync<InvalidOperationException>(() => setupService.SetupAsync(setupContext1));

        // Assert - Tracker should be released even after exception.
        Assert.False(await setupTracker.IsSetupInProgressAsync(shellSettings));
    }

    private static SetupContext CreateSetupContext(ShellSettings shellSettings)
    {
        return new SetupContext
        {
            ShellSettings = shellSettings,
            EnabledFeatures = null,
            Errors = new Dictionary<string, string>(),
            Recipe = new RecipeDescriptor { Name = "TestRecipe" },
            Properties = new Dictionary<string, object>
            {
                { SetupConstants.SiteName, "Test Site" },
                { SetupConstants.AdminUsername, "admin" },
                { SetupConstants.AdminEmail, "admin@test.com" },
                { SetupConstants.AdminPassword, "Password1!" },
                { SetupConstants.SiteTimeZone, "UTC" },
                { SetupConstants.DatabaseProvider, DatabaseProviderValue.Sqlite },
                { SetupConstants.DatabaseConnectionString, string.Empty },
                { SetupConstants.DatabaseTablePrefix, string.Empty },
                { SetupConstants.DatabaseSchema, string.Empty },
            },
        };
    }

    /// <summary>
    /// Creates a real SetupTracker backed by a mock IShellHost that returns false for
    /// TryGetSettings (no Default shell), so only the local dictionary path is exercised.
    /// </summary>
    private static SetupTracker CreateRealTracker()
    {
        var trackerShellHost = new Mock<IShellHost>();
        return new SetupTracker(trackerShellHost.Object);
    }

    private static SetupService CreateSetupService(
        IShellHost shellHost,
        IDbConnectionValidator dbConnectionValidator)
    {
        var setupTracker = new Mock<ISetupTracker>();
        setupTracker
            .Setup(x => x.TryMarkSetupStartedAsync(It.IsAny<ShellSettings>()))
            .ReturnsAsync(true);
        setupTracker
            .Setup(x => x.MarkSetupCompletedAsync(It.IsAny<ShellSettings>()))
            .Returns(Task.CompletedTask);

        return CreateSetupService(shellHost, dbConnectionValidator, setupTracker.Object);
    }

    private static SetupService CreateSetupService(
        IShellHost shellHost,
        IDbConnectionValidator dbConnectionValidator,
        ISetupTracker setupTracker)
    {
        var hostEnvironment = new Mock<IHostEnvironment>();
        hostEnvironment.Setup(x => x.ApplicationName).Returns("OrchardCore.Cms.Web");

        var setupUserIdGenerator = new Mock<ISetupUserIdGenerator>();
        setupUserIdGenerator
            .Setup(x => x.GenerateUniqueId())
            .Returns("test-user-id");

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(new DefaultHttpContext());

        var applicationLifetime = new Mock<IHostApplicationLifetime>();

        var stringLocalizer = new Mock<IStringLocalizer<SetupService>>();
        stringLocalizer
            .Setup(x => x[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));
        stringLocalizer
            .Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] args) => new LocalizedString(name, string.Format(name, args)));

        return new SetupService(
            shellHost,
            hostEnvironment.Object,
            Mock.Of<IShellContextFactory>(),
            setupUserIdGenerator.Object,
            [],
            Mock.Of<ILogger<SetupService>>(),
            stringLocalizer.Object,
            applicationLifetime.Object,
            httpContextAccessor.Object,
            dbConnectionValidator,
            setupTracker);
    }
}

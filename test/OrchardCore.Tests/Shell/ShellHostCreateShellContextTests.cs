using Microsoft.Extensions.Logging;
using Moq;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Tests.Shell;

public class ShellHostCreateShellContextTests
{
    [Fact]
    public async Task RunningTenant_WithDatabaseProvider_CreatesShellContext()
    {
        // Arrange - A Running tenant with DatabaseProvider configured should create a normal shell context.
        var settings = new ShellSettings { Name = "TestTenant" }.AsRunning();
        settings["DatabaseProvider"] = "Sqlite";

        var shellContextFactory = new Mock<IShellContextFactory>();
        shellContextFactory
            .Setup(x => x.CreateShellContextAsync(It.IsAny<ShellSettings>()))
            .ReturnsAsync(new ShellContext { Settings = settings });

        var shellHost = CreateShellHost(shellContextFactory.Object);

        // Act
        var context = await shellHost.GetOrCreateShellContextAsync(settings);

        // Assert
        Assert.NotNull(context);
        shellContextFactory.Verify(x => x.CreateShellContextAsync(settings), Times.Once);
        shellContextFactory.Verify(x => x.CreateSetupContextAsync(It.IsAny<ShellSettings>()), Times.Never);
    }

    [Fact]
    public async Task RunningTenant_WithoutDatabaseProvider_AndNoConnectionString_RecoversSqlite()
    {
        // Arrange - A Running tenant missing DatabaseProvider but with no ConnectionString
        // should be recovered to Sqlite (since it's the only file-based provider).
        var settings = new ShellSettings { Name = "TestTenant" }.AsRunning();
        // Explicitly no DatabaseProvider and no ConnectionString set.

        var shellContextFactory = new Mock<IShellContextFactory>();
        shellContextFactory
            .Setup(x => x.CreateShellContextAsync(It.IsAny<ShellSettings>()))
            .ReturnsAsync(new ShellContext { Settings = settings });

        var shellSettingsManager = new Mock<IShellSettingsManager>();
        shellSettingsManager
            .Setup(x => x.SaveSettingsAsync(It.IsAny<ShellSettings>()))
            .Returns(Task.CompletedTask);

        var shellHost = CreateShellHost(shellContextFactory.Object, shellSettingsManager.Object);

        // Act
        var context = await shellHost.GetOrCreateShellContextAsync(settings);

        // Assert - Should recover DatabaseProvider to Sqlite and create a normal shell context.
        Assert.NotNull(context);
        Assert.Equal("Sqlite", settings["DatabaseProvider"]);
        shellSettingsManager.Verify(x => x.SaveSettingsAsync(settings), Times.Once);
        shellContextFactory.Verify(x => x.CreateShellContextAsync(settings), Times.Once);
        shellContextFactory.Verify(x => x.CreateSetupContextAsync(It.IsAny<ShellSettings>()), Times.Never);
    }

    [Fact]
    public async Task RunningTenant_WithoutDatabaseProvider_ButWithConnectionString_ResetsToUninitialized()
    {
        // Arrange - A Running tenant with a ConnectionString but no DatabaseProvider
        // can't be safely recovered. It should be reset to Uninitialized.
        var settings = new ShellSettings { Name = "TestTenant" }.AsRunning();
        settings["ConnectionString"] = "Server=localhost;Database=Test;";
        // No DatabaseProvider set.

        var shellContextFactory = new Mock<IShellContextFactory>();
        shellContextFactory
            .Setup(x => x.CreateSetupContextAsync(It.IsAny<ShellSettings>()))
            .ReturnsAsync(new ShellContext { Settings = settings });

        var shellSettingsManager = new Mock<IShellSettingsManager>();
        shellSettingsManager
            .Setup(x => x.SaveSettingsAsync(It.IsAny<ShellSettings>()))
            .Returns(Task.CompletedTask);

        var shellHost = CreateShellHost(shellContextFactory.Object, shellSettingsManager.Object);

        // Act
        var context = await shellHost.GetOrCreateShellContextAsync(settings);

        // Assert - Should reset to Uninitialized and create a setup context.
        Assert.NotNull(context);
        Assert.True(settings.IsUninitialized());
        shellSettingsManager.Verify(x => x.SaveSettingsAsync(settings), Times.Once);
        shellContextFactory.Verify(x => x.CreateSetupContextAsync(settings), Times.Once);
    }

    [Fact]
    public async Task UninitializedTenant_CreatesSetupContext()
    {
        // Arrange
        var settings = new ShellSettings { Name = "TestTenant" }.AsUninitialized();

        var shellContextFactory = new Mock<IShellContextFactory>();
        shellContextFactory
            .Setup(x => x.CreateSetupContextAsync(It.IsAny<ShellSettings>()))
            .ReturnsAsync(new ShellContext { Settings = settings });

        var shellHost = CreateShellHost(shellContextFactory.Object);

        // Act
        var context = await shellHost.GetOrCreateShellContextAsync(settings);

        // Assert
        Assert.NotNull(context);
        shellContextFactory.Verify(x => x.CreateSetupContextAsync(settings), Times.Once);
        shellContextFactory.Verify(x => x.CreateShellContextAsync(It.IsAny<ShellSettings>()), Times.Never);
    }

    [Fact]
    public async Task RunningTenant_WithEmptyConnectionString_RecoversSqlite()
    {
        // Arrange - Empty string is equivalent to no connection string (SQLite uses file-based path).
        var settings = new ShellSettings { Name = "TestTenant" }.AsRunning();
        settings["ConnectionString"] = "";
        // No DatabaseProvider set.

        var shellContextFactory = new Mock<IShellContextFactory>();
        shellContextFactory
            .Setup(x => x.CreateShellContextAsync(It.IsAny<ShellSettings>()))
            .ReturnsAsync(new ShellContext { Settings = settings });

        var shellSettingsManager = new Mock<IShellSettingsManager>();
        shellSettingsManager
            .Setup(x => x.SaveSettingsAsync(It.IsAny<ShellSettings>()))
            .Returns(Task.CompletedTask);

        var shellHost = CreateShellHost(shellContextFactory.Object, shellSettingsManager.Object);

        // Act
        var context = await shellHost.GetOrCreateShellContextAsync(settings);

        // Assert
        Assert.Equal("Sqlite", settings["DatabaseProvider"]);
        shellContextFactory.Verify(x => x.CreateShellContextAsync(settings), Times.Once);
    }

    private static ShellHost CreateShellHost(
        IShellContextFactory shellContextFactory,
        IShellSettingsManager shellSettingsManager = null)
    {
        shellSettingsManager ??= Mock.Of<IShellSettingsManager>();

        return new ShellHost(
            shellSettingsManager,
            shellContextFactory,
            new RunningShellTable(),
            Mock.Of<IExtensionManager>(),
            Mock.Of<ILogger<ShellHost>>());
    }
}

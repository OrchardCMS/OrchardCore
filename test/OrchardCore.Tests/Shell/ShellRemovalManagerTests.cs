using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Removing;

namespace OrchardCore.Tests.Shell;

public class ShellRemovalManagerTests
{
    [Fact]
    public async Task RemoveAsync_ShouldBlockRemoval_WhenSetupIsInProgress()
    {
        // Arrange - tenant is Uninitialized (normally removable) but setup is tracked as in-progress.
        var shellSettings = new ShellSettings { Name = "TestTenant" }.AsUninitialized();

        var setupTracker = new Mock<ISetupTracker>();
        setupTracker
            .Setup(x => x.IsSetupInProgressAsync(It.Is<ShellSettings>(s => s.Name == "TestTenant")))
            .ReturnsAsync(true);

        var shellHost = new Mock<IShellHost>();

        var manager = CreateShellRemovalManager(shellHost.Object, setupTracker.Object);

        // Act
        var context = await manager.RemoveAsync(shellSettings);

        // Assert - Removal should be blocked by the setup tracker.
        Assert.False(context.Success);
        Assert.NotNull(context.ErrorMessage);
    }

    [Fact]
    public async Task RemoveAsync_ShouldAllowRemoval_WhenSetupIsNotInProgress_ForUninitializedTenant()
    {
        // Arrange
        var shellSettings = new ShellSettings { Name = "TestTenant" }.AsUninitialized();

        var setupTracker = new Mock<ISetupTracker>();
        setupTracker
            .Setup(x => x.IsSetupInProgressAsync(It.IsAny<ShellSettings>()))
            .ReturnsAsync(false);

        var shellHost = new Mock<IShellHost>();
        // TryGetShellContext returns false by default, so IsShellActive returns false.

        var manager = CreateShellRemovalManager(shellHost.Object, setupTracker.Object);

        // Act
        var context = await manager.RemoveAsync(shellSettings);

        // Assert - Should pass the setup-in-progress check.
        Assert.True(context.Success);
    }

    [Fact]
    public async Task RemoveAsync_ShouldAllowRemoval_AfterSetupCompletes()
    {
        // Arrange
        var shellSettings = new ShellSettings { Name = "TestTenant" }.AsUninitialized();

        var setupTracker = new Mock<ISetupTracker>();
        setupTracker
            .Setup(x => x.IsSetupInProgressAsync(It.IsAny<ShellSettings>()))
            .ReturnsAsync(false);

        var shellHost = new Mock<IShellHost>();

        var manager = CreateShellRemovalManager(shellHost.Object, setupTracker.Object);

        // Act
        var context = await manager.RemoveAsync(shellSettings);

        // Assert - Should not be blocked by tracker.
        Assert.True(context.Success);
    }

    [Fact]
    public async Task RemoveAsync_ShouldBlockRemoval_ForDefaultShell()
    {
        // Arrange
        var shellSettings = new ShellSettings { Name = ShellSettings.DefaultShellName }.AsUninitialized();

        var setupTracker = new Mock<ISetupTracker>();
        setupTracker
            .Setup(x => x.IsSetupInProgressAsync(It.IsAny<ShellSettings>()))
            .ReturnsAsync(false);

        var shellHost = new Mock<IShellHost>();

        var manager = CreateShellRemovalManager(shellHost.Object, setupTracker.Object);

        // Act
        var context = await manager.RemoveAsync(shellSettings);

        // Assert
        Assert.False(context.Success);
        Assert.NotNull(context.ErrorMessage);
    }

    private static ShellRemovalManager CreateShellRemovalManager(
        IShellHost shellHost,
        ISetupTracker setupTracker)
    {
        var localizer = new Mock<IStringLocalizer<ShellRemovalManager>>();
        localizer
            .Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] args) => new LocalizedString(name, string.Format(name, args)));

        return new ShellRemovalManager(
            shellHost,
            Mock.Of<IShellContextFactory>(),
            [],
            setupTracker,
            localizer.Object,
            Mock.Of<ILogger<ShellRemovalManager>>());
    }
}

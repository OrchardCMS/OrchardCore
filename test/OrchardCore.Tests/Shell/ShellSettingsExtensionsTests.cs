using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Tests.Shell;

public class ShellSettingsExtensionsTests
{
    [Theory]
    [InlineData(TenantState.Uninitialized, true)]
    [InlineData(TenantState.Initializing, false)]
    [InlineData(TenantState.Disabled, true)]
    [InlineData(TenantState.Running, false)]
    [InlineData(TenantState.Invalid, false)]
    public void IsRemovable_ShouldReturnExpectedResult(TenantState state, bool expectedRemovable)
    {
        // Arrange
        var settings = new ShellSettings { Name = "TestTenant", State = state };

        // Act
        var result = settings.IsRemovable();

        // Assert
        Assert.Equal(expectedRemovable, result);
    }

    [Theory]
    [InlineData(TenantState.Uninitialized, true)]
    [InlineData(TenantState.Initializing, true)]
    [InlineData(TenantState.Disabled, false)]
    [InlineData(TenantState.Running, false)]
    [InlineData(TenantState.Invalid, false)]
    public void IsSetupable_ShouldReturnExpectedResult(TenantState state, bool expectedSetupable)
    {
        // Arrange
        var settings = new ShellSettings { Name = "TestTenant", State = state };

        // Act
        var result = settings.IsSetupable();

        // Assert
        Assert.Equal(expectedSetupable, result);
    }

    [Fact]
    public void IsRemovable_ShouldNotAllowRemovalOfInitializingTenant()
    {
        // Arrange - A tenant in Initializing state (setup is in progress).
        var settings = new ShellSettings { Name = "ActiveSetupTenant" }.AsInitializing();

        // Act & Assert - Should NOT be removable since setup is in progress.
        // The ISetupTracker provides the active-setup guard separately.
        Assert.False(settings.IsRemovable());
    }

    [Fact]
    public void IsSetupable_ShouldAllowRetryForInitializingTenant()
    {
        // Arrange - A tenant stuck in Initializing state after a failed setup.
        var settings = new ShellSettings { Name = "StuckTenant" }.AsInitializing();

        // Act & Assert - Should be setupable so the setup can be retried.
        Assert.True(settings.IsSetupable());
    }

    [Fact]
    public void IsSetupable_ShouldNotAllowSetupForRunningTenant()
    {
        // Arrange
        var settings = new ShellSettings { Name = "RunningTenant" }.AsRunning();

        // Act & Assert
        Assert.False(settings.IsSetupable());
    }

    [Fact]
    public void IsSetupable_ShouldNotAllowSetupForDisabledTenant()
    {
        // Arrange
        var settings = new ShellSettings { Name = "DisabledTenant" }.AsDisabled();

        // Act & Assert
        Assert.False(settings.IsSetupable());
    }
}

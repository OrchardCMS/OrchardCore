using Moq;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Tests.Shell;

public class SetupTrackerTests
{
    private static SetupTracker CreateTracker()
    {
        // Mock IShellHost that returns false for TryGetSettings (no Default shell available),
        // so only the local ConcurrentDictionary path is exercised.
        var shellHost = new Mock<IShellHost>();
        return new SetupTracker(shellHost.Object);
    }

    [Fact]
    public async Task TryMarkSetupStarted_ShouldReturnTrue_WhenNoSetupInProgress()
    {
        var tracker = CreateTracker();
        var settings = new ShellSettings { Name = "Tenant1" };

        var result = await tracker.TryMarkSetupStartedAsync(settings);

        Assert.True(result);
        Assert.True(await tracker.IsSetupInProgressAsync(settings));
    }

    [Fact]
    public async Task TryMarkSetupStarted_ShouldReturnFalse_WhenSetupAlreadyInProgress()
    {
        var tracker = CreateTracker();
        var settings = new ShellSettings { Name = "Tenant1" };
        await tracker.TryMarkSetupStartedAsync(settings);

        var result = await tracker.TryMarkSetupStartedAsync(settings);

        Assert.False(result);
    }

    [Fact]
    public async Task MarkSetupCompleted_ShouldAllowNewSetup()
    {
        var tracker = CreateTracker();
        var settings = new ShellSettings { Name = "Tenant1" };
        await tracker.TryMarkSetupStartedAsync(settings);
        await tracker.MarkSetupCompletedAsync(settings);

        Assert.False(await tracker.IsSetupInProgressAsync(settings));
        Assert.True(await tracker.TryMarkSetupStartedAsync(settings));
    }

    [Fact]
    public async Task IsSetupInProgress_ShouldBeCaseInsensitive()
    {
        var tracker = CreateTracker();
        await tracker.TryMarkSetupStartedAsync(new ShellSettings { Name = "Tenant1" });

        Assert.True(await tracker.IsSetupInProgressAsync(new ShellSettings { Name = "tenant1" }));
        Assert.True(await tracker.IsSetupInProgressAsync(new ShellSettings { Name = "TENANT1" }));
    }

    [Fact]
    public async Task DifferentTenants_ShouldBeIndependent()
    {
        var tracker = CreateTracker();
        var settings1 = new ShellSettings { Name = "Tenant1" };
        var settings2 = new ShellSettings { Name = "Tenant2" };
        await tracker.TryMarkSetupStartedAsync(settings1);

        Assert.False(await tracker.IsSetupInProgressAsync(settings2));
        Assert.True(await tracker.TryMarkSetupStartedAsync(settings2));
    }

    [Fact]
    public async Task MarkSetupCompleted_ShouldBeIdempotent()
    {
        var tracker = CreateTracker();
        var settings = new ShellSettings { Name = "Tenant1" };
        await tracker.TryMarkSetupStartedAsync(settings);

        // Calling MarkSetupCompletedAsync multiple times should not throw.
        await tracker.MarkSetupCompletedAsync(settings);
        await tracker.MarkSetupCompletedAsync(settings);

        Assert.False(await tracker.IsSetupInProgressAsync(settings));
    }

    [Fact]
    public async Task IsSetupInProgress_ShouldReturnFalse_ForUnknownTenant()
    {
        var tracker = CreateTracker();

        Assert.False(await tracker.IsSetupInProgressAsync(new ShellSettings { Name = "NonExistent" }));
    }
}

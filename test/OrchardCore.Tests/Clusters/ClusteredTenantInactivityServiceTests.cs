using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrchardCore.Clusters;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Tests.Clusters;

public class ClusteredTenantInactivityServiceTests
{
    [Fact]
    public async Task Release_MaxIdleTimeNotConfigured_DoesNotRelease()
    {
        var shellHost = await RunAsync(maxIdleTime: null, hasPipeline: true, idle: TimeSpan.FromDays(1));

        shellHost.Verify(host => host.ReleaseShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Release_NoPipeline_DoesNotRelease()
    {
        var shellHost = await RunAsync(TimeSpan.FromMinutes(1), hasPipeline: false, idle: TimeSpan.FromDays(1));

        shellHost.Verify(host => host.ReleaseShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Release_MaxIdleTimeNotExpired_DoesNotRelease()
    {
        var shellHost = await RunAsync(TimeSpan.FromHours(1), hasPipeline: true, idle: TimeSpan.FromMinutes(1));

        shellHost.Verify(host => host.ReleaseShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Release_PipelineWithoutRequest_DoesNotReleaseBeforeMaxIdleTime()
    {
        var shellHost = await RunAsync(TimeSpan.FromHours(1), hasPipeline: true, idle: null);

        shellHost.Verify(host => host.ReleaseShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Release_MaxIdleTimeTooLargeToAdd_DoesNotRelease()
    {
        var shellHost = await RunAsync(TimeSpan.MaxValue, hasPipeline: true, idle: TimeSpan.FromDays(1));

        shellHost.Verify(host => host.ReleaseShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Release_MaxIdleTimeExpired_ReleasesWithoutEvent()
    {
        var shellHost = await RunAsync(TimeSpan.FromMinutes(1), hasPipeline: true, idle: TimeSpan.FromHours(1));

        shellHost.Verify(host => host.ReleaseShellContextAsync(It.IsAny<ShellSettings>(), false), Times.Once);
    }

    [Fact]
    public async Task Release_SeveralTenants_OnlyReleasesTheIdleOnes()
    {
        var idle = CreateContext("Idle", hasPipeline: true, TimeSpan.FromHours(1));
        var active = CreateContext("Active", hasPipeline: true, TimeSpan.FromSeconds(1));
        var asleep = CreateContext("Asleep", hasPipeline: false, TimeSpan.FromHours(1));

        var shellHost = await RunAsync(TimeSpan.FromMinutes(1), idle, active, asleep);

        shellHost.Verify(host => host.ReleaseShellContextAsync(idle.Settings, false), Times.Once);
        shellHost.Verify(host => host.ReleaseShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()), Times.Once);
    }

    private static Task<Mock<IShellHost>> RunAsync(TimeSpan? maxIdleTime, bool hasPipeline, TimeSpan? idle) =>
        RunAsync(maxIdleTime, CreateContext("Tenant", hasPipeline, idle));

    private static ShellContext CreateContext(string name, bool hasPipeline, TimeSpan? idle)
    {
        var shellContext = new ShellContext
        {
            Settings = new ShellSettings { Name = name },
            Pipeline = hasPipeline ? Mock.Of<IShellPipeline>() : null,
        };

        if (idle.HasValue)
        {
            shellContext.LastRequestTimeUtc = DateTime.UtcNow - idle.Value;
        }

        return shellContext;
    }

    private static async Task<Mock<IShellHost>> RunAsync(TimeSpan? maxIdleTime, params ShellContext[] shellContexts)
    {
        var shellHost = new Mock<IShellHost>();
        shellHost.Setup(host => host.ListShellContexts()).Returns(shellContexts);

        var monitor = new Mock<IOptionsMonitor<ClustersOptions>>();
        monitor.SetupGet(m => m.CurrentValue).Returns(new ClustersOptions { MaxIdleTime = maxIdleTime });

        await new ClusteredTenantInactivityService(shellHost.Object, monitor.Object, NullLogger<ClusteredTenantInactivityService>.Instance)
            .ReleaseIdleTenantsAsync();

        return shellHost;
    }
}

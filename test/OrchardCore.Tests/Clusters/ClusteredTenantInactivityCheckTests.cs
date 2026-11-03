using Microsoft.Extensions.Options;
using OrchardCore.Clusters;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Tests.Clusters;

public class ClusteredTenantInactivityCheckTests
{
    [Fact]
    public async Task DoWork_MaxIdleTimeNotConfigured_DoesNotRelease()
    {
        var shellHost = await RunAsync(maxIdleTime: null, hasPipeline: true, idle: TimeSpan.FromDays(1));

        shellHost.Verify(host => host.ReleaseShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task DoWork_NoPipeline_DoesNotRelease()
    {
        var shellHost = await RunAsync(TimeSpan.FromMinutes(1), hasPipeline: false, idle: TimeSpan.FromDays(1));

        shellHost.Verify(host => host.ReleaseShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task DoWork_MaxIdleTimeNotExpired_DoesNotRelease()
    {
        var shellHost = await RunAsync(TimeSpan.FromHours(1), hasPipeline: true, idle: TimeSpan.FromMinutes(1));

        shellHost.Verify(host => host.ReleaseShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task DoWork_PipelineWithoutRequest_DoesNotReleaseBeforeMaxIdleTime()
    {
        var shellHost = await RunAsync(TimeSpan.FromHours(1), hasPipeline: true, idle: null);

        shellHost.Verify(host => host.ReleaseShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task DoWork_MaxIdleTimeTooLargeToAdd_DoesNotRelease()
    {
        var shellHost = await RunAsync(TimeSpan.MaxValue, hasPipeline: true, idle: TimeSpan.FromDays(1));

        shellHost.Verify(host => host.ReleaseShellContextAsync(It.IsAny<ShellSettings>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task DoWork_MaxIdleTimeExpired_ReleasesWithoutEvent()
    {
        var shellHost = await RunAsync(TimeSpan.FromMinutes(1), hasPipeline: true, idle: TimeSpan.FromHours(1));

        shellHost.Verify(host => host.ReleaseShellContextAsync(It.IsAny<ShellSettings>(), false), Times.Once);
    }

    private static async Task<Mock<IShellHost>> RunAsync(TimeSpan? maxIdleTime, bool hasPipeline, TimeSpan? idle)
    {
        var settings = new ShellSettings { Name = "Tenant" };
        var shellContext = new ShellContext
        {
            Settings = settings,
            Pipeline = hasPipeline ? Mock.Of<IShellPipeline>() : null,
        };

        if (idle.HasValue)
        {
            shellContext.LastRequestTimeUtc = DateTime.UtcNow - idle.Value;
        }

        var shellHost = new Mock<IShellHost>();
        shellHost.Setup(host => host.TryGetShellContext(settings.Name, out shellContext)).Returns(true);

        var monitor = new Mock<IOptionsMonitor<ClustersOptions>>();
        monitor.SetupGet(m => m.CurrentValue).Returns(new ClustersOptions { MaxIdleTime = maxIdleTime });

        await new ClusteredTenantInactivityCheck(shellHost.Object, settings, monitor.Object)
            .DoWorkAsync(null, TestContext.Current.CancellationToken);

        return shellHost;
    }
}

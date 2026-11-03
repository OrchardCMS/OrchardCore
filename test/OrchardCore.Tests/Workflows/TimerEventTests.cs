using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Timers;

namespace OrchardCore.Tests.Workflows;

public class TimerEventTests
{
    [Fact]
    public async Task CanExecuteAsync_AfterTheTimerIsDue_IsTrueTheFirstTimeTheTaskLooksAtIt()
    {
        var now = new DateTime(2026, 1, 1, 10, 0, 30, DateTimeKind.Utc);
        var clock = new Mock<IClock>();
        clock.SetupGet(x => x.UtcNow).Returns(now);

        var timer = new TimerEvent(clock.Object, Mock.Of<ISiteService>(), NullLogger<TimerEvent>.Instance, Mock.Of<IStringLocalizerFactory>(), Mock.Of<IStringLocalizer<TimerEvent>>())
        {
            CronExpression = "* * * * *",
        };

        // The workflow starts waiting on the timer.
        var result = timer.Execute(null, null);
        Assert.True(result.IsHalted);

        // The task's first run is after the next minute: the timer is due then, without a run to start it first.
        clock.SetupGet(x => x.UtcNow).Returns(now.AddSeconds(40));

        Assert.True(await timer.CanExecuteAsync(null, null));
    }

    [Fact]
    public async Task CanExecuteAsync_BeforeTheTimerIsDue_IsFalse()
    {
        var now = new DateTime(2026, 1, 1, 10, 0, 30, DateTimeKind.Utc);
        var clock = new Mock<IClock>();
        clock.SetupGet(x => x.UtcNow).Returns(now);

        var timer = new TimerEvent(clock.Object, Mock.Of<ISiteService>(), NullLogger<TimerEvent>.Instance, Mock.Of<IStringLocalizerFactory>(), Mock.Of<IStringLocalizer<TimerEvent>>())
        {
            CronExpression = "* * * * *",
        };

        timer.Execute(null, null);
        clock.SetupGet(x => x.UtcNow).Returns(now.AddSeconds(10));

        Assert.False(await timer.CanExecuteAsync(null, null));
    }
}

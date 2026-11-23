using Microsoft.Extensions.Logging;
using OrchardCore.Jobs;
using OrchardCore.Jobs.Models;
using OrchardCore.Jobs.Services;
using OrchardCore.Modules;

namespace OrchardCore.Tests.Modules.OrchardCore.Jobs;

public sealed class DefaultJobExecutionContextTests
{
    [Fact]
    public void Apply_SaveThatDidNotSucceed_CopiesTheReportsAgain()
    {
        // Arrange
        var context = new DefaultJobExecutionContext(new JobRun(), new Clock());

        context.LogInformation("First");
        context.Log(LogLevel.Warning, "Second");
        context.ReportProgress(40, "Almost half way");

        // Act: the save of the first run conflicts with another change, so the run is read and changed again.
        var conflicting = new JobRun();
        var saved = new JobRun();

        context.Apply(conflicting);
        context.Apply(saved);

        // Assert
        Assert.Equal(["First", "Second"], saved.Log.Select(entry => entry.Message));
        Assert.Equal(40, saved.Progress);
        Assert.Equal("Almost half way", saved.ProgressMessage);
    }

    [Fact]
    public void Apply_AfterASave_OnlyCopiesWhatWasReportedSince()
    {
        // Arrange
        var context = new DefaultJobExecutionContext(new JobRun(), new Clock());
        var first = new JobRun();

        context.LogInformation("First");
        context.ReportProgress(40);
        context.Apply(first);

        // Reported while the run was being saved.
        context.LogInformation("Second");
        context.Saved();

        // Act
        var second = new JobRun { Progress = 75, ProgressMessage = "Set by the run that was read" };
        context.Apply(second);

        // Assert
        Assert.Equal(["Second"], second.Log.Select(entry => entry.Message));

        // The progress didn't change since it was saved, so the one of the run is kept.
        Assert.Equal(75, second.Progress);
        Assert.Equal("Set by the run that was read", second.ProgressMessage);
    }

    [Fact]
    public void Apply_LogFull_DoesNotGoOverItsLimit()
    {
        // Arrange
        var context = new DefaultJobExecutionContext(new JobRun(), new Clock());

        for (var i = 0; i < JobRun.MaxLogEntries + 10; i++)
        {
            context.LogInformation($"Entry {i}");
        }

        // Act
        var run = new JobRun();
        context.Apply(run);
        context.Apply(run);

        // Assert
        Assert.Equal(JobRun.MaxLogEntries, run.Log.Count);
    }
}

using OrchardCore.DataOrchestrator.BackgroundTasks;
using OrchardCore.DataOrchestrator.Models;
using OrchardCore.DataOrchestrator.Services;
using OrchardCore.Modules;

namespace OrchardCore.Tests.Modules.OrchardCore.DataOrchestrator;

public class EtlPipelineBackgroundTaskTests
{
    [Fact]
    public async Task DoWorkAsync_ExecutesOnlyPipelinesWhoseScheduleIsDue()
    {
        var now = new DateTime(2026, 06, 29, 12, 30, 00, DateTimeKind.Utc);
        var duePipeline = new EtlPipelineDefinition
        {
            PipelineId = "due",
            Name = "Due",
            IsEnabled = true,
            Schedule = "*/10 * * * *",
            LastRunUtc = now.AddMinutes(-10),
        };
        var notDuePipeline = new EtlPipelineDefinition
        {
            PipelineId = "not-due",
            Name = "Not due",
            IsEnabled = true,
            Schedule = "0 0 * * *",
            LastRunUtc = now.AddHours(-1),
        };
        var invalidSchedulePipeline = new EtlPipelineDefinition
        {
            PipelineId = "invalid",
            Name = "Invalid",
            IsEnabled = true,
            Schedule = "not a cron",
            LastRunUtc = now.AddDays(-1),
        };

        var pipelineService = new Mock<IEtlPipelineService>(MockBehavior.Strict);
        pipelineService
            .Setup(x => x.ListEnabledAsync())
            .ReturnsAsync([duePipeline, notDuePipeline, invalidSchedulePipeline]);
        pipelineService
            .Setup(x => x.SaveLogAsync(It.Is<EtlExecutionLog>(log => log.PipelineId == duePipeline.PipelineId)))
            .Returns(Task.CompletedTask);
        pipelineService
            .Setup(x => x.SaveAsync(It.Is<EtlPipelineDefinition>(pipeline => pipeline.PipelineId == duePipeline.PipelineId)))
            .Returns(Task.CompletedTask);

        var executor = new Mock<IEtlPipelineExecutor>(MockBehavior.Strict);
        executor
            .Setup(x => x.ExecuteAsync(
                duePipeline,
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EtlExecutionLog
            {
                PipelineId = duePipeline.PipelineId,
                PipelineName = duePipeline.Name,
                StartedUtc = now,
                CompletedUtc = now,
                Status = "Success",
            });

        var serviceProvider = new ServiceCollection()
            .AddSingleton(pipelineService.Object)
            .AddSingleton(executor.Object)
            .AddSingleton(Mock.Of<IClock>(clock => clock.UtcNow == now))
            .AddSingleton<ILogger<EtlPipelineBackgroundTask>>(NullLogger<EtlPipelineBackgroundTask>.Instance)
            .BuildServiceProvider();

        var task = new EtlPipelineBackgroundTask();

        await task.DoWorkAsync(serviceProvider, TestContext.Current.CancellationToken);

        Assert.Equal(now, duePipeline.LastRunUtc);
        Assert.Equal(now.AddHours(-1), notDuePipeline.LastRunUtc);

        executor.Verify(x => x.ExecuteAsync(
            It.Is<EtlPipelineDefinition>(pipeline => pipeline.PipelineId == duePipeline.PipelineId),
            It.IsAny<IDictionary<string, object>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        executor.VerifyNoOtherCalls();
        pipelineService.Verify(x => x.SaveAsync(duePipeline), Times.Once);
        pipelineService.Verify(x => x.SaveLogAsync(It.Is<EtlExecutionLog>(log => log.PipelineId == duePipeline.PipelineId)), Times.Once);
        pipelineService.Verify(x => x.ListEnabledAsync(), Times.Once);
        pipelineService.VerifyNoOtherCalls();
    }
}

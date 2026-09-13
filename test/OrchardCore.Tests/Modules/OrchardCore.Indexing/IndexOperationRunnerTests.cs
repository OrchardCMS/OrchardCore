using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Operations;
using OrchardCore.Modules;
using OrchardCore.Tests.Apis.Context;
using YesSql;

namespace OrchardCore.Tests.Modules.OrchardCore.Indexing;

public class IndexOperationRunnerTests
{
    [Theory]
    [InlineData("completed")]
    [InlineData("failed")]
    [InlineData("exception")]
    [InlineData("late")]
    [InlineData("unverified")]
    [InlineData("expired")]
    public async Task Run_ConfirmedOutcome_IsPersistedWithoutRepeatingWork(string scenario)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var now = DateTime.UtcNow;
            var clock = new Mock<IClock>();
            clock.SetupGet(value => value.UtcNow).Returns(() => now);
            var store = new IndexOperationStore(scope.ServiceProvider.GetRequiredService<IStore>(), clock.Object);
            var operation = await store.CreateAsync("index", IndexLifecycleAction.Rebuild);
            var executor = new Mock<IIndexOperationExecutor>();
            IndexOperationRunner runner = null;
            executor.Setup(value => value.ExecuteAsync("index", IndexLifecycleAction.Rebuild)).Returns(async () =>
            {
                Assert.Equal(IndexOperationState.Running, (await store.FindAsync(operation.OperationId)).State);
                if (scenario == "exception") { throw new InvalidOperationException("Provider detail must not be persisted"); }
                if (scenario == "late")
                {
                    now = now.AddMinutes(31);
                    Assert.Equal(IndexOperationState.Uncertain, (await runner.FindAsync(operation.OperationId)).State);
                }
                return new IndexProcessingResult("index", scenario == "expired" ? IndexProcessingStatus.LockExpired
                    : scenario == "unverified" ? IndexProcessingStatus.Unverified
                    : scenario == "failed" ? IndexProcessingStatus.Busy : IndexProcessingStatus.Completed, 42);
            });
            runner = new IndexOperationRunner(store, executor.Object, clock.Object, new HttpContextAccessor(), NullLogger<IndexOperationRunner>.Instance);

            await runner.RunAsync(operation.OperationId);
            await runner.RunAsync(operation.OperationId);

            var result = await store.FindAsync(operation.OperationId);
            var completed = scenario is "completed" or "late";
            Assert.Equal(completed ? IndexOperationState.Completed
                : scenario is "unverified" or "expired" ? IndexOperationState.Uncertain : IndexOperationState.Failed, result.State);
            Assert.Equal(scenario == "exception" ? IndexProcessingStatus.Failed
                : scenario == "expired" ? IndexProcessingStatus.LockExpired
                    : scenario == "unverified" ? IndexProcessingStatus.Unverified
                : scenario == "failed" ? IndexProcessingStatus.Busy : IndexProcessingStatus.Completed, result.Outcome);
            Assert.Equal(scenario == "exception" ? null : (long?)42, result.LastTaskId);
            executor.Verify(value => value.ExecuteAsync("index", IndexLifecycleAction.Rebuild), Times.Once);
        });
    }

    [Fact]
    public async Task Run_OverduePendingOperation_RecordsUncertaintyWithoutExecuting()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var now = DateTime.UtcNow;
            var clock = new Mock<IClock>();
            clock.SetupGet(value => value.UtcNow).Returns(() => now);
            var store = new IndexOperationStore(scope.ServiceProvider.GetRequiredService<IStore>(), clock.Object);
            var operation = await store.CreateAsync("index", IndexLifecycleAction.Reset);
            now = now.AddMinutes(31);
            var executor = new Mock<IIndexOperationExecutor>(MockBehavior.Strict);
            var runner = new IndexOperationRunner(store, executor.Object, clock.Object, new HttpContextAccessor(), NullLogger<IndexOperationRunner>.Instance);

            await runner.RunAsync(operation.OperationId);

            Assert.Equal(IndexOperationState.Uncertain, (await runner.FindAsync(operation.OperationId)).State);
            executor.VerifyNoOtherCalls();
        });
    }
}

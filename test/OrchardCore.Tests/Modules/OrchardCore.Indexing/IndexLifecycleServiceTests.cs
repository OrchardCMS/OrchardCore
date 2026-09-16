using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.Tests.Modules.OrchardCore.Indexing;

public class IndexLifecycleServiceTests
{
    [Theory]
    [InlineData(IndexLifecycleAction.Synchronize)]
    [InlineData(IndexLifecycleAction.Reset)]
    [InlineData(IndexLifecycleAction.Rebuild)]
    public async Task Execute_Action_PreparesAndProcessesUnderOneLock(IndexLifecycleAction action)
    {
        var calls = new List<string>();
        var profile = new IndexProfile { Id = "index", Type = "Test", ProviderName = "Test", IndexFullName = "index" };
        var profiles = new Mock<IIndexProfileManager>();
        profiles.Setup(manager => manager.FindByIdAsync(profile.Id)).ReturnsAsync(profile);
        profiles.Setup(manager => manager.ResetAsync(profile)).Callback(() => calls.Add("reset"));
        profiles.Setup(manager => manager.UpdateAsync(profile, null)).Callback(() => calls.Add("update"));
        var store = new Mock<IIndexProfileStore>();
        var tasks = new Mock<IIndexingTaskManager>();
        tasks.Setup(manager => manager.GetIndexingTasksAsync(0, It.IsAny<int>(), "Test"))
            .Callback(() => calls.Add("tasks")).ReturnsAsync([]);
        var indexes = new Mock<IIndexManager>();
        indexes.Setup(manager => manager.RebuildAsync(profile)).Callback(() => calls.Add("rebuild")).ReturnsAsync(true);
        indexes.Setup(manager => manager.ExistsAsync("index")).Callback(() => calls.Add("exists")).ReturnsAsync(true);
        var documents = new Mock<IDocumentIndexManager>();
        documents.Setup(manager => manager.GetLastTaskIdAsync(profile)).Callback(() => calls.Add("cursor")).ReturnsAsync(0);
        var locker = new Mock<ILocker>();
        locker.Setup(value => value.DisposeAsync()).Callback(() => calls.Add("release"));
        var locking = new Mock<IDistributedLock>();
        locking.Setup(value => value.TryAcquireLockAsync("IndexingService-index", It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .Callback(() => calls.Add("lock")).ReturnsAsync((locker.Object, true));
        var handler = new Mock<IIndexProfileHandler>();
        handler.Setup(value => value.SynchronizedAsync(It.IsAny<IndexProfileSynchronizedContext>()))
            .Callback<IndexProfileSynchronizedContext>(context =>
            {
                Assert.True(context.IsIndexingCompleted);
                Assert.Same(profile, context.IndexProfile);
                calls.Add("synchronized");
            });
        using var services = new ServiceCollection().AddSingleton(handler.Object).AddSingleton(locking.Object)
            .AddKeyedSingleton<IIndexManager>("Test", indexes.Object)
            .AddKeyedSingleton<IDocumentIndexManager>("Test", documents.Object)
            .AddKeyedSingleton<NamedIndexingService>("Test", (provider, _) => new Processor(store.Object, tasks.Object, provider))
            .BuildServiceProvider();
        var lifecycle = new IndexLifecycleService(profiles.Object, services);

        var result = await lifecycle.ExecuteAsync(profile.Id, action);

        Assert.Equal(IndexProcessingStatus.Completed, result.Status);
        var expected = new List<string> { "lock" };
        if (action == IndexLifecycleAction.Rebuild) { expected.Add("rebuild"); }
        if (action != IndexLifecycleAction.Synchronize) { expected.AddRange(["reset", "update"]); }
        expected.AddRange(["exists", "cursor", "tasks", "release", "synchronized"]);
        Assert.Equal(expected, calls);
        locking.Verify(value => value.TryAcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Theory]
    [InlineData("rejected")]
    [InlineData("provider-error")]
    [InlineData("reset-error")]
    public async Task Rebuild_FailedPreparation_ReleasesLockWithoutProcessingTasks(string failure)
    {
        var profile = new IndexProfile { Id = "index", Type = "Test", ProviderName = "Test", IndexFullName = "index" };
        var profiles = new Mock<IIndexProfileManager>();
        profiles.Setup(manager => manager.FindByIdAsync(profile.Id)).ReturnsAsync(profile);
        var indexes = new Mock<IIndexManager>();
        if (failure == "provider-error")
        {
            indexes.Setup(manager => manager.RebuildAsync(profile)).ThrowsAsync(new InvalidOperationException("Provider failed."));
        }
        else
        {
            indexes.Setup(manager => manager.RebuildAsync(profile)).ReturnsAsync(failure != "rejected");
        }
        if (failure == "reset-error")
        {
            profiles.Setup(manager => manager.ResetAsync(profile)).ThrowsAsync(new InvalidOperationException("Reset failed."));
        }
        var tasks = new Mock<IIndexingTaskManager>(MockBehavior.Strict);
        var documents = new Mock<IDocumentIndexManager>(MockBehavior.Strict);
        var locker = new Mock<ILocker>();
        var locking = new Mock<IDistributedLock>();
        locking.Setup(value => value.TryAcquireLockAsync("IndexingService-index", It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync((locker.Object, true));
        using var services = new ServiceCollection().AddSingleton(locking.Object)
            .AddKeyedSingleton<IIndexManager>("Test", indexes.Object)
            .AddKeyedSingleton<IDocumentIndexManager>("Test", documents.Object)
            .AddKeyedSingleton<NamedIndexingService>("Test", (provider, _) => new Processor(Mock.Of<IIndexProfileStore>(), tasks.Object, provider))
            .BuildServiceProvider();
        var lifecycle = new IndexLifecycleService(profiles.Object, services);

        if (failure == "rejected")
        {
            var result = await lifecycle.ExecuteAsync(profile.Id, IndexLifecycleAction.Rebuild);
            Assert.Equal(IndexProcessingStatus.ProviderRejected, result.Status);
            Assert.Null(result.LastTaskId);
        }
        else
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => lifecycle.ExecuteAsync(profile.Id, IndexLifecycleAction.Rebuild));
        }

        profiles.Verify(manager => manager.ResetAsync(profile), failure == "reset-error" ? Times.Once() : Times.Never());
        profiles.Verify(manager => manager.UpdateAsync(It.IsAny<IndexProfile>(), It.IsAny<System.Text.Json.Nodes.JsonNode>()), Times.Never());
        indexes.Verify(manager => manager.ExistsAsync(It.IsAny<string>()), Times.Never());
        tasks.VerifyNoOtherCalls();
        documents.VerifyNoOtherCalls();
        locker.Verify(value => value.DisposeAsync(), Times.Once());
    }

    [Theory]
    [InlineData(IndexLifecycleAction.Synchronize, false)]
    [InlineData(IndexLifecycleAction.Reset, false)]
    [InlineData(IndexLifecycleAction.Rebuild, false)]
    [InlineData(IndexLifecycleAction.Rebuild, true)]
    public async Task LegacySource_PreservesHandlersWithoutClaimingCompletion(IndexLifecycleAction action, bool rejected)
    {
        var calls = new List<string>();
        var profile = new IndexProfile { Id = "legacy", Type = "Legacy", ProviderName = "Test" };
        var profiles = new Mock<IIndexProfileManager>();
        profiles.Setup(value => value.FindByIdAsync(profile.Id)).ReturnsAsync(profile);
        profiles.Setup(value => value.ResetAsync(profile)).Callback(() => calls.Add("reset"));
        profiles.Setup(value => value.UpdateAsync(profile, null)).Callback(() => calls.Add("update"));
        var provider = new Mock<IIndexManager>();
        provider.Setup(value => value.RebuildAsync(profile)).Callback(() => calls.Add("rebuild")).ReturnsAsync(!rejected);
        var locker = new Mock<ILocker>();
        locker.Setup(value => value.DisposeAsync()).Callback(() => calls.Add("release"));
        var locking = new Mock<IDistributedLock>();
        locking.Setup(value => value.TryAcquireLockAsync("IndexingService-legacy", It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .Callback(() => calls.Add("lock")).ReturnsAsync((locker.Object, true));
        var handler = new Mock<IIndexProfileHandler>();
        handler.Setup(value => value.SynchronizedAsync(It.IsAny<IndexProfileSynchronizedContext>()))
            .Callback<IndexProfileSynchronizedContext>(context =>
            {
                Assert.False(context.IsIndexingCompleted);
                Assert.Same(profile, context.IndexProfile);
                calls.Add("synchronized");
            });
        using var services = new ServiceCollection().AddSingleton(locking.Object).AddSingleton(handler.Object)
            .AddKeyedSingleton<IIndexManager>("Test", provider.Object).BuildServiceProvider();

        var result = await new IndexLifecycleService(profiles.Object, services).ExecuteAsync(profile.Id, action);

        Assert.Equal(rejected ? IndexProcessingStatus.ProviderRejected : IndexProcessingStatus.Unverified, result.Status);
        Assert.Null(result.LastTaskId);
        var expected = new List<string>();
        if (action != IndexLifecycleAction.Synchronize) { expected.Add("lock"); }
        if (action == IndexLifecycleAction.Rebuild) { expected.Add("rebuild"); }
        if (!rejected && action != IndexLifecycleAction.Synchronize) { expected.AddRange(["reset", "update"]); }
        if (action != IndexLifecycleAction.Synchronize) { expected.Add("release"); }
        if (!rejected) { expected.Add("synchronized"); }
        Assert.Equal(expected, calls);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Rebuild_LeaseExpiresInProvider_DoesNotResetOrNotify(bool legacy, bool throws)
    {
        long elapsed = 0;
        var time = new Mock<TimeProvider>();
        time.SetupGet(value => value.TimestampFrequency).Returns(TimeSpan.TicksPerSecond);
        time.Setup(value => value.GetTimestamp()).Returns(() => elapsed);
        var profile = new IndexProfile { Id = "index", Type = "Test", ProviderName = "Test" };
        var profiles = new Mock<IIndexProfileManager>(MockBehavior.Strict);
        profiles.Setup(value => value.FindByIdAsync(profile.Id)).ReturnsAsync(profile);
        var indexes = new Mock<IIndexManager>(MockBehavior.Strict);
        indexes.Setup(value => value.RebuildAsync(profile)).Returns(() =>
        {
            elapsed = TimeSpan.FromMinutes(16).Ticks;
            return throws ? Task.FromException<bool>(new InvalidOperationException("Provider expired.")) : Task.FromResult(true);
        });
        var locker = new Mock<ILocker>();
        var locking = new Mock<IDistributedLock>();
        locking.Setup(value => value.TryAcquireLockAsync("IndexingService-index", It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync((locker.Object, true));
        var handler = new Mock<IIndexProfileHandler>(MockBehavior.Strict);
        var services = new ServiceCollection().AddSingleton<TimeProvider>(time.Object).AddSingleton(locking.Object)
            .AddSingleton(handler.Object).AddKeyedSingleton<IIndexManager>("Test", indexes.Object)
            .AddKeyedSingleton<IDocumentIndexManager>("Test", Mock.Of<IDocumentIndexManager>());
        if (!legacy)
        {
            services.AddKeyedSingleton<NamedIndexingService>("Test", (provider, _) =>
                new Processor(Mock.Of<IIndexProfileStore>(), Mock.Of<IIndexingTaskManager>(), provider));
        }
        using var provider = services.BuildServiceProvider();

        var result = await new IndexLifecycleService(profiles.Object, provider).ExecuteAsync(profile.Id, IndexLifecycleAction.Rebuild);

        Assert.Equal(IndexProcessingStatus.LockExpired, result.Status);
        Assert.Null(result.LastTaskId);
        profiles.Verify(value => value.FindByIdAsync(profile.Id), Times.Once());
        profiles.VerifyNoOtherCalls();
        handler.VerifyNoOtherCalls();
        locker.Verify(value => value.DisposeAsync(), Times.Once());
    }

    [Fact]
    public async Task ContentHandler_AlreadyProcessed_DoesNotStartAnotherProcessor()
    {
        // A missing processor makes accidental re-entry fail instead of masking a second run.
        var handler = new global::OrchardCore.Indexing.Core.Handlers.ContentIndexProfileHandler(null, null, null);
        await handler.SynchronizedAsync(new IndexProfileSynchronizedContext(new IndexProfile
        {
            Id = "index", Type = IndexingConstants.ContentsIndexSource,
        }) { IsIndexingCompleted = true });
    }

    private sealed class Processor : NamedIndexingService
    {
        public Processor(IIndexProfileStore store, IIndexingTaskManager tasks, IServiceProvider services)
            : base("Test", store, tasks, [], services, NullLogger.Instance)
        {
        }

        protected override Task<BuildDocumentIndexContext> GetBuildDocumentIndexAsync(IndexProfileEntryContext entry, RecordIndexingTask task)
            => throw new InvalidOperationException("The fixture queue must be empty.");
    }
}

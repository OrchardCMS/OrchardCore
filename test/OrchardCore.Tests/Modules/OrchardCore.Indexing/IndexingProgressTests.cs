using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.Tests.Modules.OrchardCore.Indexing;

public class IndexingProgressTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ProcessRecords_FailedBatch_DoesNotAdvancePastFailure(bool handlerFailure)
    {
        var profile = new IndexProfile { Id = "index", Name = "Index", ProviderName = "Test", Type = "Test", IndexFullName = "index" };
        var profiles = new Mock<IIndexProfileStore>();
        profiles.Setup(store => store.GetByTypeAsync("Test")).ReturnsAsync([profile]);
        var tasks = new Mock<IIndexingTaskManager>();
        tasks.Setup(manager => manager.GetIndexingTasksAsync(It.IsAny<long>(), It.IsAny<int>(), "Test"))
            .ReturnsAsync((long after, int count, string category) => after < 2
                ? new[] { new RecordIndexingTask { Id = after + 1, RecordId = "record" + after, Category = category } }
                : []);
        var documents = new Mock<IDocumentIndexManager>();
        if (handlerFailure)
        {
            documents.Setup(manager => manager.AddOrUpdateDocumentsAsync(profile, It.IsAny<IEnumerable<DocumentIndex>>())).ReturnsAsync(true);
        }
        else
        {
            documents.SetupSequence(manager => manager.AddOrUpdateDocumentsAsync(profile, It.IsAny<IEnumerable<DocumentIndex>>()))
                .ReturnsAsync(false).ReturnsAsync(true);
        }
        var handler = new Mock<IDocumentIndexHandler>();
        handler.SetupSequence(value => value.BuildIndexAsync(It.IsAny<BuildDocumentIndexContext>()))
            .Returns(handlerFailure ? Task.FromException(new InvalidOperationException("Document build failed")) : Task.CompletedTask)
            .Returns(Task.CompletedTask);
        using var services = CreateServices(documents.Object);
        var indexing = new TestIndexingService(profiles.Object, tasks.Object, [handler.Object], services);

        await indexing.ProcessRecordsAsync([profile.Id]);

        documents.Verify(manager => manager.SetLastTaskIdAsync(profile, It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task ProcessRecords_FilteredBatch_AdvancesCursorWithoutProviderWrite()
    {
        var profile = new IndexProfile { Id = "index", Name = "Index", ProviderName = "Test", Type = "Test", IndexFullName = "index" };
        var profiles = new Mock<IIndexProfileStore>();
        profiles.Setup(store => store.GetByTypeAsync("Test")).ReturnsAsync([profile]);
        var tasks = new Mock<IIndexingTaskManager>();
        tasks.Setup(manager => manager.GetIndexingTasksAsync(It.IsAny<long>(), It.IsAny<int>(), "Test"))
            .ReturnsAsync((long after, int count, string category) => after == 0
                ? new[] { new RecordIndexingTask { Id = 1, RecordId = "filtered", Category = category } }
                : []);
        var documents = new Mock<IDocumentIndexManager>();
        using var services = CreateServices(documents.Object);
        var indexing = new TestIndexingService(profiles.Object, tasks.Object, [], services) { FilterDocuments = true };

        await indexing.ProcessRecordsAsync([profile.Id]);

        documents.Verify(manager => manager.SetLastTaskIdAsync(profile, 1), Times.Once);
        documents.Verify(manager => manager.AddOrUpdateDocumentsAsync(profile, It.IsAny<IEnumerable<DocumentIndex>>()), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ProcessRecords_OneIndexFails_OtherIndexContinues(bool throws)
    {
        var failed = new IndexProfile { Id = "failed", Name = "Failed", ProviderName = "Test", Type = "Test", IndexFullName = "index" };
        var healthy = new IndexProfile { Id = "healthy", Name = "Healthy", ProviderName = "Test", Type = "Test", IndexFullName = "index" };
        var profiles = new Mock<IIndexProfileStore>();
        profiles.Setup(store => store.GetByTypeAsync("Test")).ReturnsAsync([failed, healthy]);
        var tasks = new Mock<IIndexingTaskManager>();
        tasks.Setup(manager => manager.GetIndexingTasksAsync(It.IsAny<long>(), It.IsAny<int>(), "Test"))
            .ReturnsAsync((long after, int count, string category) => after < 2
                ? new[] { new RecordIndexingTask { Id = after + 1, RecordId = "record" + after, Category = category } }
                : []);
        var documents = new Mock<IDocumentIndexManager>();
        documents.Setup(manager => manager.AddOrUpdateDocumentsAsync(healthy, It.IsAny<IEnumerable<DocumentIndex>>())).ReturnsAsync(true);
        documents.Setup(manager => manager.AddOrUpdateDocumentsAsync(failed, It.IsAny<IEnumerable<DocumentIndex>>()))
            .Returns(() => throws ? Task.FromException<bool>(new InvalidOperationException("Provider failed")) : Task.FromResult(false));
        using var services = CreateServices(documents.Object);
        var indexing = new TestIndexingService(profiles.Object, tasks.Object, [], services);

        var results = await indexing.ProcessRecordsWithResultsAsync([failed.Id, healthy.Id]);

        Assert.Equal(IndexProcessingStatus.Failed, results.Single(result => result.IndexId == failed.Id).Status);
        var completed = results.Single(result => result.IndexId == healthy.Id);
        Assert.Equal(IndexProcessingStatus.Completed, completed.Status);
        Assert.Equal(2, completed.LastTaskId);
        documents.Verify(manager => manager.SetLastTaskIdAsync(failed, It.IsAny<long>()), Times.Never);
        documents.Verify(manager => manager.SetLastTaskIdAsync(healthy, 2), Times.Once);
        documents.Verify(manager => manager.AddOrUpdateDocumentsAsync(failed, It.IsAny<IEnumerable<DocumentIndex>>()), Times.Once);
    }

    [Fact]
    public async Task ProcessRecords_ReadCursor_HoldsIndexLock()
    {
        var profile = new IndexProfile { Id = "index", Name = "Index", ProviderName = "Test", Type = "Test", IndexFullName = "index" };
        var profiles = new Mock<IIndexProfileStore>();
        profiles.Setup(store => store.GetByTypeAsync("Test")).ReturnsAsync([profile]);
        var tasks = new Mock<IIndexingTaskManager>();
        tasks.Setup(manager => manager.GetIndexingTasksAsync(It.IsAny<long>(), It.IsAny<int>(), "Test")).ReturnsAsync([]);
        var acquired = false;
        var locking = new Mock<IDistributedLock>();
        var locker = new Mock<ILocker>();
        locking.Setup(value => value.TryAcquireLockAsync("IndexingService-index", It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .Callback(() => acquired = true).ReturnsAsync((locker.Object, true));
        var documents = new Mock<IDocumentIndexManager>();
        documents.Setup(manager => manager.GetLastTaskIdAsync(profile))
            .Callback(() => Assert.True(acquired, "Cursor must be read after acquiring the lock to avoid a concurrent reset."))
            .ReturnsAsync(0);
        using var services = CreateServices(documents.Object, locking);
        var indexing = new TestIndexingService(profiles.Object, tasks.Object, [], services);

        await indexing.ProcessRecordsAsync([profile.Id]);

        locker.Verify(value => value.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task ProcessRecords_ProviderCannotAnswer_IsReportedUnavailableNotMissing()
    {
        var profile = new IndexProfile { Id = "index", Name = "Index", ProviderName = "Test", Type = "Test", IndexFullName = "index" };
        var profiles = new Mock<IIndexProfileStore>();
        profiles.Setup(store => store.GetByTypeAsync("Test")).ReturnsAsync([profile]);
        var indexes = new Mock<IIndexManager>();
        indexes.As<IRemoteIndexManager>().Setup(manager => manager.TryGetExistsAsync("index")).ReturnsAsync((bool?)null);
        var locking = new Mock<IDistributedLock>();
        locking.Setup(value => value.TryAcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(() => (Mock.Of<ILocker>(), true));
        using var services = new ServiceCollection().AddSingleton(TimeProvider.System).AddSingleton(locking.Object)
            .AddKeyedSingleton<IDocumentIndexManager>("Test", Mock.Of<IDocumentIndexManager>())
            .AddKeyedSingleton<IIndexManager>("Test", indexes.Object).BuildServiceProvider();
        var tasks = new Mock<IIndexingTaskManager>(MockBehavior.Strict);
        var indexing = new TestIndexingService(profiles.Object, tasks.Object, [], services);

        var result = Assert.Single(await indexing.ProcessRecordsWithResultsAsync([profile.Id]));

        Assert.Equal(IndexProcessingStatus.ProviderUnavailable, result.Status);
        Assert.Null(result.LastTaskId);
    }

    [Theory]
    [InlineData(IndexProcessingStatus.NotFound)]
    [InlineData(IndexProcessingStatus.Busy)]
    [InlineData(IndexProcessingStatus.ProviderUnavailable)]
    [InlineData(IndexProcessingStatus.ProviderMissing)]
    public async Task ProcessRecords_SkippedIndex_IsNotReportedCompleted(IndexProcessingStatus expected)
    {
        var profile = new IndexProfile { Id = "index", Name = "Index", ProviderName = "Test", Type = "Test", IndexFullName = "index" };
        var profiles = new Mock<IIndexProfileStore>();
        profiles.Setup(store => store.GetByTypeAsync("Test")).ReturnsAsync(expected == IndexProcessingStatus.NotFound ? [] : [profile]);
        var documents = new Mock<IDocumentIndexManager>();
        var locking = new Mock<IDistributedLock>();
        locking.Setup(value => value.TryAcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(() => (Mock.Of<ILocker>(), expected != IndexProcessingStatus.Busy));
        using var services = expected == IndexProcessingStatus.ProviderUnavailable
            ? new ServiceCollection().AddSingleton(locking.Object).BuildServiceProvider()
            : CreateServices(documents.Object, locking, expected != IndexProcessingStatus.ProviderMissing);
        var tasks = new Mock<IIndexingTaskManager>(MockBehavior.Strict);
        var indexing = new TestIndexingService(profiles.Object, tasks.Object, [], services);

        var result = Assert.Single(await indexing.ProcessRecordsWithResultsAsync([profile.Id]));

        Assert.Equal(profile.Id, result.IndexId);
        Assert.Equal(expected, result.Status);
        Assert.Null(result.LastTaskId);
        documents.Verify(manager => manager.SetLastTaskIdAsync(profile, It.IsAny<long>()), Times.Never);
    }

    [Theory]
    [InlineData("handler")]
    [InlineData("provider")]
    [InlineData("empty-queue")]
    public async Task ProcessRecords_ExpiredLease_DoesNotAdvanceCursorOrReportCompletion(string stage)
    {
        long elapsed = 0;
        var time = new Mock<TimeProvider>();
        time.SetupGet(value => value.TimestampFrequency).Returns(TimeSpan.TicksPerSecond);
        time.Setup(value => value.GetTimestamp()).Returns(() => elapsed);
        var profile = new IndexProfile { Id = "index", Type = "Test", ProviderName = "Test", IndexFullName = "index" };
        var profiles = new Mock<IIndexProfileStore>();
        profiles.Setup(value => value.GetByTypeAsync("Test")).ReturnsAsync([profile]);
        var tasks = new Mock<IIndexingTaskManager>();
        tasks.Setup(value => value.GetIndexingTasksAsync(It.IsAny<long>(), It.IsAny<int>(), "Test"))
            .Callback(() => { if (stage == "empty-queue") { elapsed = TimeSpan.FromMinutes(16).Ticks; } })
            .ReturnsAsync((long after, int count, string category) => after == 0 && stage != "empty-queue"
                ? new[] { new RecordIndexingTask { Id = 1, RecordId = "record", Category = category } } : []);
        var documents = new Mock<IDocumentIndexManager>();
        documents.Setup(value => value.AddOrUpdateDocumentsAsync(profile, It.IsAny<IEnumerable<DocumentIndex>>()))
            .Callback(() => { if (stage == "provider") { elapsed = TimeSpan.FromMinutes(16).Ticks; } }).ReturnsAsync(true);
        var handler = new Mock<IDocumentIndexHandler>();
        handler.Setup(value => value.BuildIndexAsync(It.IsAny<BuildDocumentIndexContext>()))
            .Callback(() => { if (stage == "handler") { elapsed = TimeSpan.FromMinutes(16).Ticks; } });
        using var services = CreateServices(documents.Object, time: time.Object);
        var indexing = new TestIndexingService(profiles.Object, tasks.Object, [handler.Object], services);

        var result = Assert.Single(await indexing.ProcessRecordsWithResultsAsync([profile.Id]));

        Assert.Equal(IndexProcessingStatus.LockExpired, result.Status);
        Assert.Equal(0, result.LastTaskId);
        documents.Verify(value => value.SetLastTaskIdAsync(profile, It.IsAny<long>()), Times.Never());
        documents.Verify(value => value.AddOrUpdateDocumentsAsync(profile, It.IsAny<IEnumerable<DocumentIndex>>()),
            stage == "provider" ? Times.Once() : Times.Never());
    }

    [Fact]
    public async Task ProcessRecords_DocumentKeepsFailing_SkipsItAfterMaxAttemptsAndAdvances()
    {
        var failures = new FakeFailureStore();
        var (indexing, documents, profile) = CreatePoisonScenario(failures, new InvalidOperationException("Document build failed"), "record1");

        for (var attempt = 1; attempt < PoisonIndexingService.Attempts; attempt++)
        {
            var failed = Assert.Single(await indexing.ProcessRecordsWithResultsAsync([profile.Id]));
            Assert.Equal(IndexProcessingStatus.Failed, failed.Status);
            Assert.Equal(0, failed.SkippedTasks);
        }
        documents.Verify(manager => manager.SetLastTaskIdAsync(profile, It.IsAny<long>()), Times.Never);

        var result = Assert.Single(await indexing.ProcessRecordsWithResultsAsync([profile.Id]));

        Assert.Equal(IndexProcessingStatus.Completed, result.Status);
        Assert.Equal(1, result.SkippedTasks);
        Assert.Equal(3, result.LastTaskId);
        documents.Verify(manager => manager.AddOrUpdateDocumentsAsync(profile, It.Is<IEnumerable<DocumentIndex>>(batch =>
            batch.Select(document => document.Id).SequenceEqual(new[] { "record0", "record2" }))), Times.Once);
        documents.Verify(manager => manager.SetLastTaskIdAsync(profile, 3), Times.Once);
        Assert.Equal(1, failures.Cleared);
        Assert.Empty(failures.Attempts);
    }

    [Fact]
    public async Task ProcessRecords_TwoDocumentsKeepFailingInOneBatch_SkipsBothAndAdvances()
    {
        var failures = new FakeFailureStore();
        var (indexing, documents, profile) = CreatePoisonScenario(failures, new InvalidOperationException("Document build failed"), "record0", "record2");

        IndexProcessingResult result = null;
        for (var run = 0; run < PoisonIndexingService.Attempts * 2 && result?.Status != IndexProcessingStatus.Completed; run++)
        {
            result = Assert.Single(await indexing.ProcessRecordsWithResultsAsync([profile.Id]));
        }

        Assert.Equal(IndexProcessingStatus.Completed, result.Status);
        Assert.Equal(2, result.SkippedTasks);
        documents.Verify(manager => manager.SetLastTaskIdAsync(profile, 3), Times.Once);
    }

    public static TheoryData<Exception> OutageExceptions =>
    [
        new HttpRequestException("The service is down."),
        new TimeoutException("The service timed out."),
        new TaskCanceledException("The request was canceled."),
        new InvalidOperationException("Wrapped", new HttpRequestException("The service is down.")),
    ];

    [Theory]
    [MemberData(nameof(OutageExceptions))]
    public async Task ProcessRecords_HandlerFailsFromOutage_NeverSkipsTheDocument(Exception outage)
    {
        var failures = new FakeFailureStore();
        var (indexing, documents, profile) = CreatePoisonScenario(failures, outage, "record1");

        for (var run = 0; run < PoisonIndexingService.Attempts * 2; run++)
        {
            Assert.Equal(IndexProcessingStatus.Failed, Assert.Single(await indexing.ProcessRecordsWithResultsAsync([profile.Id])).Status);
        }

        documents.Verify(manager => manager.SetLastTaskIdAsync(profile, It.IsAny<long>()), Times.Never);
        Assert.Empty(failures.Attempts);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ProcessRecords_ProviderKeepsFailing_NeverSkipsAnyDocument(bool throws)
    {
        var failures = new FakeFailureStore();
        var (indexing, documents, profile) = CreatePoisonScenario(failures, new InvalidOperationException("Unused"));
        documents.Setup(manager => manager.AddOrUpdateDocumentsAsync(profile, It.IsAny<IEnumerable<DocumentIndex>>()))
            .Returns(() => throws ? Task.FromException<bool>(new InvalidOperationException("Provider failed")) : Task.FromResult(false));

        for (var run = 0; run < PoisonIndexingService.Attempts * 2; run++)
        {
            Assert.Equal(IndexProcessingStatus.Failed, Assert.Single(await indexing.ProcessRecordsWithResultsAsync([profile.Id])).Status);
        }

        documents.Verify(manager => manager.SetLastTaskIdAsync(profile, It.IsAny<long>()), Times.Never);
        Assert.Empty(failures.Attempts);
    }

    [Fact]
    public async Task ProcessRecords_FailureStoreUnavailable_KeepsRetryingTheDocument()
    {
        var failures = new FakeFailureStore { Unavailable = true };
        var (indexing, documents, profile) = CreatePoisonScenario(failures, new InvalidOperationException("Document build failed"), "record1");

        for (var run = 0; run < PoisonIndexingService.Attempts * 2; run++)
        {
            Assert.Equal(IndexProcessingStatus.Failed, Assert.Single(await indexing.ProcessRecordsWithResultsAsync([profile.Id])).Status);
        }

        documents.Verify(manager => manager.SetLastTaskIdAsync(profile, It.IsAny<long>()), Times.Never);
    }

    private static (PoisonIndexingService Indexing, Mock<IDocumentIndexManager> Documents, IndexProfile Profile) CreatePoisonScenario(
        FakeFailureStore failures, Exception exception, params string[] failingRecords)
    {
        var profile = new IndexProfile { Id = "index", Name = "Index", ProviderName = "Test", Type = "Test", IndexFullName = "index" };
        var profiles = new Mock<IIndexProfileStore>();
        profiles.Setup(store => store.GetByTypeAsync("Test")).ReturnsAsync([profile]);
        var tasks = new Mock<IIndexingTaskManager>();
        tasks.Setup(manager => manager.GetIndexingTasksAsync(It.IsAny<long>(), It.IsAny<int>(), "Test"))
            .ReturnsAsync((long after, int count, string category) => after == 0
                ? Enumerable.Range(0, 3).Select(i => new RecordIndexingTask { Id = i + 1, RecordId = "record" + i, Category = category }).ToArray()
                : []);
        var documents = new Mock<IDocumentIndexManager>();
        documents.Setup(manager => manager.AddOrUpdateDocumentsAsync(profile, It.IsAny<IEnumerable<DocumentIndex>>())).ReturnsAsync(true);
        var handler = new Mock<IDocumentIndexHandler>();
        handler.Setup(value => value.BuildIndexAsync(It.IsAny<BuildDocumentIndexContext>()))
            .Returns((BuildDocumentIndexContext context) => failingRecords.Contains(context.DocumentIndex.Id)
                ? Task.FromException(exception)
                : Task.CompletedTask);
        var services = new ServiceCollection().AddSingleton<IIndexingFailureStore>(failures);
        var indexes = new Mock<IIndexManager>();
        indexes.Setup(manager => manager.ExistsAsync("index")).ReturnsAsync(true);
        var locking = new Mock<IDistributedLock>();
        locking.Setup(value => value.TryAcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(() => (Mock.Of<ILocker>(), true));
        services.AddSingleton(locking.Object).AddKeyedSingleton<IDocumentIndexManager>("Test", documents.Object)
            .AddKeyedSingleton<IIndexManager>("Test", indexes.Object);
        return (new PoisonIndexingService(profiles.Object, tasks.Object, [handler.Object], services.BuildServiceProvider()), documents, profile);
    }

    private static ServiceProvider CreateServices(IDocumentIndexManager documents, Mock<IDistributedLock> locking = null, bool providerExists = true, TimeProvider time = null)
    {
        var indexes = new Mock<IIndexManager>();
        indexes.Setup(manager => manager.ExistsAsync("index")).ReturnsAsync(providerExists);
        if (locking is null)
        {
            locking = new Mock<IDistributedLock>();
            locking.Setup(value => value.TryAcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(() => (Mock.Of<ILocker>(), true));
        }
        return new ServiceCollection().AddSingleton(time ?? TimeProvider.System).AddSingleton(locking.Object)
            .AddKeyedSingleton<IDocumentIndexManager>("Test", documents)
            .AddKeyedSingleton<IIndexManager>("Test", indexes.Object).BuildServiceProvider();
    }

    private sealed class FakeFailureStore : IIndexingFailureStore
    {
        public Dictionary<long, int> Attempts { get; } = [];
        public int Cleared { get; private set; }
        public bool Unavailable { get; init; }

        public Task<int> RecordFailureAsync(string indexId, long taskId)
        {
            if (Unavailable)
            {
                return Task.FromResult(0);
            }
            Attempts[taskId] = Attempts.GetValueOrDefault(taskId) + 1;
            return Task.FromResult(Attempts[taskId]);
        }

        public Task ClearAsync(string indexId)
        {
            Cleared++;
            Attempts.Clear();
            return Task.CompletedTask;
        }
    }

    private sealed class PoisonIndexingService : NamedIndexingService
    {
        public const int Attempts = 3;

        protected override int MaxDocumentAttempts => Attempts;

        public PoisonIndexingService(IIndexProfileStore profiles, IIndexingTaskManager tasks,
            IEnumerable<IDocumentIndexHandler> handlers, IServiceProvider services)
            : base("Test", profiles, tasks, handlers, services, NullLogger.Instance)
        {
        }

        protected override Task<BuildDocumentIndexContext> GetBuildDocumentIndexAsync(IndexProfileEntryContext entry, RecordIndexingTask task)
            => Task.FromResult(new BuildDocumentIndexContext(new DocumentIndex(task.RecordId), new object(), [], null));
    }

    private sealed class TestIndexingService : NamedIndexingService
    {
        public bool FilterDocuments { get; init; }

        public TestIndexingService(IIndexProfileStore profiles, IIndexingTaskManager tasks,
            IEnumerable<IDocumentIndexHandler> handlers, IServiceProvider services)
            : base("Test", profiles, tasks, handlers, services, NullLogger.Instance)
        {
        }

        protected override Task<BuildDocumentIndexContext> GetBuildDocumentIndexAsync(IndexProfileEntryContext entry, RecordIndexingTask task)
            => Task.FromResult(FilterDocuments ? null : new BuildDocumentIndexContext(new DocumentIndex(task.RecordId), new object(), [], null));
    }
}

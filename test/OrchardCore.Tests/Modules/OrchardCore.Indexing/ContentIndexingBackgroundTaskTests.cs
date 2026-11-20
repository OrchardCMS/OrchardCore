using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Jobs;
using OrchardCore.Indexing.Models;
using OrchardCore.Jobs;
using OrchardCore.Jobs.Models;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Search.Indexing.Core;

namespace OrchardCore.Tests.Modules.OrchardCore.Indexing;

public sealed class ContentIndexingBackgroundTaskTests
{
    private const string LocalProvider = "LocalProvider";
    private const string SharedProvider = "SharedProvider";
    private const long LocalCursor = 20;
    private const long SharedCursor = 5;

    [Fact]
    public async Task DoWorkAsync_SharedIndexWithPendingTasks_QueuesOneJobInsteadOfUpdatingIt()
    {
        // Arrange
        var site = new TestSite(SharedIndex());
        site.Tasks
            .Setup(manager => manager.GetIndexingTasksAsync(SharedCursor, 1, IndexingConstants.ContentsIndexSource))
            .ReturnsAsync([new RecordIndexingTask { Id = 9 }]);

        // Act
        await new ContentIndexingBackgroundTask().DoWorkAsync(site.Services, TestContext.Current.CancellationToken);

        // Assert
        var request = Assert.Single(site.Requests);
        Assert.Equal(ContentIndexingJob.Name, request.DeduplicationKey);
        Assert.Equal(ContentIndexingJob.Name, request.ConcurrencyKey);
        Assert.False(request.RequireCurrentNode);

        // The task didn't load the tasks to index, nor update the index.
        site.Tasks.Verify(manager => manager.GetIndexingTasksAsync(It.IsAny<long>(), It.Is<int>(count => count > 1), It.IsAny<string>()), Times.Never);
        site.SharedDocuments.Verify(manager => manager.AddOrUpdateDocumentsAsync(It.IsAny<IndexProfile>(), It.IsAny<IEnumerable<DocumentIndex>>()), Times.Never);
    }

    [Fact]
    public async Task DoWorkAsync_SharedIndexUpToDate_QueuesNothing()
    {
        // Arrange
        var site = new TestSite(SharedIndex());

        // Act
        await new ContentIndexingBackgroundTask().DoWorkAsync(site.Services, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(site.Requests);
        site.SharedDocuments.Verify(manager => manager.GetLastTaskIdAsync(It.IsAny<IndexProfile>()), Times.Once);
    }

    [Fact]
    public async Task DoWorkAsync_SharedIndexThatDoesNotExist_QueuesNothing()
    {
        // Arrange
        var site = new TestSite(SharedIndex());
        site.SharedIndexes.Setup(manager => manager.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        site.Tasks
            .Setup(manager => manager.GetIndexingTasksAsync(It.IsAny<long>(), 1, IndexingConstants.ContentsIndexSource))
            .ReturnsAsync([new RecordIndexingTask { Id = 9 }]);

        // Act
        await new ContentIndexingBackgroundTask().DoWorkAsync(site.Services, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(site.Requests);
    }

    [Fact]
    public async Task DoWorkAsync_LocalIndex_UpdatesItOnThisServerWithoutAJob()
    {
        // Arrange
        var site = new TestSite(LocalIndex());

        // Act
        await new ContentIndexingBackgroundTask().DoWorkAsync(site.Services, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(site.Requests);
        site.Tasks.Verify(manager => manager.GetIndexingTasksAsync(LocalCursor, It.Is<int>(count => count > 1), IndexingConstants.ContentsIndexSource), Times.Once);
    }

    [Fact]
    public async Task DoWorkAsync_SharedProviderThatFails_StillUpdatesTheLocalIndex()
    {
        // Arrange
        var site = new TestSite(SharedIndex(), LocalIndex());
        site.SharedDocuments.Setup(manager => manager.GetLastTaskIdAsync(It.IsAny<IndexProfile>())).ThrowsAsync(new InvalidOperationException("The search service can't be reached."));

        // Act
        await new ContentIndexingBackgroundTask().DoWorkAsync(site.Services, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(site.Requests);
        site.Tasks.Verify(manager => manager.GetIndexingTasksAsync(LocalCursor, It.Is<int>(count => count > 1), IndexingConstants.ContentsIndexSource), Times.Once);
    }

    [Fact]
    public void IsLocalProvider_ProviderRegisteredAgain_KeepsWhatItDeclared()
    {
        // Arrange
        var options = new IndexingOptions();
        options.AddIndexingProvider(LocalProvider, provider => provider.IsLocal = true);

        // Act: a source of the provider is registered after it.
        options.AddIndexingSource(LocalProvider, IndexingConstants.ContentsIndexSource);
        options.AddIndexingSource(SharedProvider, IndexingConstants.ContentsIndexSource);

        // Assert
        Assert.True(options.IsLocalProvider(LocalProvider));
        Assert.False(options.IsLocalProvider(SharedProvider));
        Assert.False(options.IsLocalProvider("Unknown"));
        Assert.False(options.IsLocalProvider(null));
    }

    private static IndexProfile LocalIndex()
        => new() { Id = "local", Name = "Local", IndexName = "local", IndexFullName = "local", ProviderName = LocalProvider, Type = IndexingConstants.ContentsIndexSource };

    private static IndexProfile SharedIndex()
        => new() { Id = "shared", Name = "Shared", IndexName = "shared", IndexFullName = "shared", ProviderName = SharedProvider, Type = IndexingConstants.ContentsIndexSource };

    /// <summary>
    /// The services the task uses, with a provider whose indexes are kept by each server, and one whose indexes are shared.
    /// </summary>
    private sealed class TestSite
    {
        public TestSite(params IndexProfile[] indexProfiles)
        {
            var store = new Mock<IIndexProfileStore>();
            store.Setup(x => x.GetByTypeAsync(IndexingConstants.ContentsIndexSource)).Returns(() => ValueTask.FromResult<IEnumerable<IndexProfile>>(indexProfiles));

            // There is no task to index by default.
            Tasks.Setup(manager => manager.GetIndexingTasksAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync([]);

            LocalDocuments.Setup(manager => manager.GetLastTaskIdAsync(It.IsAny<IndexProfile>())).ReturnsAsync(LocalCursor);
            SharedDocuments.Setup(manager => manager.GetLastTaskIdAsync(It.IsAny<IndexProfile>())).ReturnsAsync(SharedCursor);
            LocalIndexes.Setup(manager => manager.ExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
            SharedIndexes.Setup(manager => manager.ExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            var jobManager = new Mock<IJobManager>();
            jobManager
                .Setup(manager => manager.EnqueueAsync<ContentIndexingJob>(It.IsAny<object>(), It.IsAny<Action<JobRequest>>()))
                .Callback<object, Action<JobRequest>>((_, configure) =>
                {
                    var request = new JobRequest();
                    configure?.Invoke(request);
                    Requests.Add(request);
                })
                .ReturnsAsync(new JobRun());

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(store.Object);
            services.AddSingleton(Tasks.Object);
            services.AddSingleton(jobManager.Object);
            services.AddSingleton<IDistributedLock, LocalLock>();
            services.AddKeyedSingleton(LocalProvider, LocalDocuments.Object);
            services.AddKeyedSingleton(SharedProvider, SharedDocuments.Object);
            services.AddKeyedSingleton(LocalProvider, LocalIndexes.Object);
            services.AddKeyedSingleton(SharedProvider, SharedIndexes.Object);
            services.Configure<IndexingOptions>(options =>
            {
                options.AddIndexingProvider(LocalProvider, provider => provider.IsLocal = true);
                options.AddIndexingProvider(SharedProvider);
            });
            services.AddSingleton(serviceProvider => new ContentIndexingService(
                Tasks.Object,
                store.Object,
                store: null,
                contentManager: null,
                serviceProvider,
                [],
                serviceProvider.GetRequiredService<ILogger<ContentIndexingService>>()));

            Services = services.BuildServiceProvider();
        }

        public IServiceProvider Services { get; }

        public Mock<IIndexingTaskManager> Tasks { get; } = new();

        public Mock<IDocumentIndexManager> LocalDocuments { get; } = new();

        public Mock<IDocumentIndexManager> SharedDocuments { get; } = new();

        public Mock<IIndexManager> LocalIndexes { get; } = new();

        public Mock<IIndexManager> SharedIndexes { get; } = new();

        public List<JobRequest> Requests { get; } = [];
    }
}

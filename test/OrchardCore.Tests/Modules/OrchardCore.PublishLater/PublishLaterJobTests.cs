using OrchardCore.ArchiveLater.Models;
using OrchardCore.ArchiveLater.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Jobs.Indexes;
using OrchardCore.Jobs.Models;
using OrchardCore.PublishLater.Models;
using OrchardCore.PublishLater.Services;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Modules.OrchardCore.Jobs;
using YesSql;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Modules.OrchardCore.PublishLater;

public sealed class PublishLaterJobTests
{
    [Fact]
    public async Task BackgroundTask_SeveralServersFindADueContentItem_PublishesItOnce()
    {
        // Arrange
        using var context = await JobsTestSite.CreateAsync("OrchardCore.PublishLater");
        string contentItemId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            await scope.ServiceProvider.GetRequiredService<IContentDefinitionManager>()
                .AlterTypeDefinitionAsync("BlogPost", type => type.WithPart(nameof(PublishLaterPart)));
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var contentItem = await contentManager.NewAsync("BlogPost");
            contentItem.DisplayText = "Published later";
            contentItem.Alter<PublishLaterPart>(part => part.ScheduledPublishUtc = DateTime.UtcNow.AddMinutes(-5));
            await contentManager.CreateAsync(contentItem, VersionOptions.Draft);
            contentItemId = contentItem.ContentItemId;
        });

        // Act: each scope stands for a server whose background task finds the content item.
        await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => Task.Run(() =>
            context.UsingTenantScopeAsync(scope => ActivatorUtilities.CreateInstance<ScheduledPublishingBackgroundTask>(scope.ServiceProvider).DoWorkAsync(scope.ServiceProvider, CancellationToken.None)))));

        var run = await JobsTestSite.WaitForLastRunAsync(context, PublishLaterJob.Name, contentItemId);

        // Assert
        Assert.True(run.Status == JobRunStatus.Succeeded, run.Error);
        Assert.Equal("Published later", run.Title);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var published = await scope.ServiceProvider.GetRequiredService<IContentManager>().GetAsync(contentItemId, VersionOptions.Published);

            Assert.NotNull(published);
            Assert.True(published.TryGet<PublishLaterPart>(out var part));
            Assert.Null(part.ScheduledPublishUtc);

            // The runs that the servers queued at the same time, if any, executed one after the other: a single one published.
            var runs = await scope.ServiceProvider.GetRequiredService<ISession>()
                .Query<JobRun, JobRunIndex>(index => index.DeduplicationKey == contentItemId, collection: JobRun.Collection)
                .ListAsync();

            Assert.Single(runs, candidate => candidate.Log.Any(entry => entry.Message.Contains("was published", StringComparison.Ordinal)));
        });

        // The content item is not due anymore: nothing is queued again.
        await context.UsingTenantScopeAsync(scope => ActivatorUtilities.CreateInstance<ScheduledPublishingBackgroundTask>(scope.ServiceProvider).DoWorkAsync(scope.ServiceProvider, CancellationToken.None));
        await context.UsingTenantScopeAsync(async scope =>
        {
            Assert.Null(await scope.ServiceProvider.GetRequiredService<global::OrchardCore.Jobs.IJobManager>().FindActiveAsync(PublishLaterJob.Name, contentItemId));
        });
    }

    [Fact]
    public async Task BackgroundTask_DueContentItem_ArchivesIt()
    {
        // Arrange
        using var context = await JobsTestSite.CreateAsync("OrchardCore.ArchiveLater");
        string contentItemId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            await scope.ServiceProvider.GetRequiredService<IContentDefinitionManager>()
                .AlterTypeDefinitionAsync("BlogPost", type => type.WithPart(nameof(ArchiveLaterPart)));
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var contentItem = await contentManager.NewAsync("BlogPost");
            contentItem.DisplayText = "Archived later";
            contentItem.Alter<ArchiveLaterPart>(part => part.ScheduledArchiveUtc = DateTime.UtcNow.AddMinutes(-5));
            await contentManager.CreateAsync(contentItem, VersionOptions.Published);
            contentItemId = contentItem.ContentItemId;
        });

        // Act
        await context.UsingTenantScopeAsync(scope => ActivatorUtilities.CreateInstance<ScheduledArchivingBackgroundTask>(scope.ServiceProvider).DoWorkAsync(scope.ServiceProvider, CancellationToken.None));

        var run = await JobsTestSite.WaitForLastRunAsync(context, ArchiveLaterJob.Name, contentItemId);

        // Assert
        Assert.True(run.Status == JobRunStatus.Succeeded, run.Error);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();

            Assert.Null(await contentManager.GetAsync(contentItemId, VersionOptions.Published));
            Assert.NotNull(await contentManager.GetAsync(contentItemId, VersionOptions.Latest));
        });
    }
}

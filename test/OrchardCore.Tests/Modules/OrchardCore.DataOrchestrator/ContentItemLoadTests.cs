using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Models;
using OrchardCore.DataOrchestrator.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.DataOrchestrator;

public class ContentItemLoadTests
{
    [Fact]
    public async Task ExecuteAsync_WithExistingContentItem_UpdatesAndPublishesWithoutCreatingDuplicate()
    {
        var existing = new ContentItem
        {
            ContentItemId = "content-item-id",
            ContentItemVersionId = "version-id",
            ContentType = "Product",
            Latest = true,
        };
        var contentManager = new Mock<IContentManager>(MockBehavior.Strict);
        contentManager
            .Setup(x => x.GetAsync(existing.ContentItemId, VersionOptions.DraftRequired))
            .ReturnsAsync(existing);
        contentManager
            .Setup(x => x.UpdateAsync(It.Is<ContentItem>(item => item.ContentItemId == existing.ContentItemId)))
            .Returns(Task.CompletedTask);
        contentManager
            .Setup(x => x.PublishAsync(It.Is<ContentItem>(item => item.ContentItemId == existing.ContentItemId)))
            .ReturnsAsync(true);

        var services = new ServiceCollection()
            .AddSingleton(contentManager.Object)
            .AddSingleton<ILogger<ContentItemLoad>>(NullLogger<ContentItemLoad>.Instance)
            .BuildServiceProvider();

        var context = new EtlExecutionContext(
            new EtlPipelineDefinition(),
            Mock.Of<IEtlActivityLibrary>(),
            services,
            NullLogger.Instance,
            TestContext.Current.CancellationToken)
        {
            DataStream = ToAsyncEnumerable(new JsonObject
            {
                ["ContentItemId"] = existing.ContentItemId,
                ["DisplayText"] = "Updated product",
            }, TestContext.Current.CancellationToken),
        };
        var load = new ContentItemLoad
        {
            ContentType = "Product",
        };

        var result = await load.ExecuteAsync(context);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, context.RecordsProcessed);
        Assert.Equal(1, context.RecordsLoaded);
        contentManager.Verify(x => x.CreateAsync(It.IsAny<ContentItem>(), It.IsAny<VersionOptions>()), Times.Never);
        contentManager.Verify(x => x.UpdateAsync(It.Is<ContentItem>(item => item.ContentItemId == existing.ContentItemId)), Times.Once);
        contentManager.Verify(x => x.PublishAsync(It.Is<ContentItem>(item => item.ContentItemId == existing.ContentItemId)), Times.Once);
    }

    private static async IAsyncEnumerable<JsonObject> ToAsyncEnumerable(
        JsonObject record,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Yield();
        yield return record;
    }
}

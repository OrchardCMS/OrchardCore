using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Modules;

namespace OrchardCore.ContentManagement.Tests;

public class DefaultContentManagerTests
{
    private readonly static DefaultContentItemIdGenerator _contentItemIdGenerator = new DefaultContentItemIdGenerator(new DefaultIdGenerator());

    private readonly Mock<TestContentHandler>  _contentHandlerMock = new Mock<TestContentHandler>();
    private readonly IEnumerable<IContentHandler> _contentHandlers;

    public DefaultContentManagerTests() => _contentHandlers = new List<IContentHandler>([_contentHandlerMock.Object]);

    [Fact]
    public async Task CreatingContentItem_ShouldNotBePublished_IfVersionNotSpecified()
    {
        // Arrange
        var contentManager = CreateContentManager();
        var contentItem = new ContentItem();

        // Act
        await contentManager.CreateAsync(contentItem);

        // Assert
        _contentHandlerMock.Verify(h => h.CreatedAsync(It.IsAny<CreateContentContext>()), Times.Once);
        _contentHandlerMock.Verify(h => h.PublishedAsync(It.IsAny<PublishContentContext>()), Times.Never);
    }

    [Fact]
    public async Task CreatingContentItem_ShouldPublished_IfPublishedVersionOptionSpecified()
    {
        // Arrange
        var contentManager = CreateContentManager();
        var contentItem = new ContentItem();

        // Act
        await contentManager.CreateAsync(contentItem, VersionOptions.Published);

        // Assert
        _contentHandlerMock.Verify(h => h.CreatedAsync(It.IsAny<CreateContentContext>()), Times.Once);
        _contentHandlerMock.Verify(h => h.PublishedAsync(It.IsAny<PublishContentContext>()), Times.Once);
    }

    [Fact]
    public async Task CreatingContentItem_ShouldBeDraft_IfDraftVersionOptionSpecified()
    {
        // Arrange
        var contentManager = CreateContentManager();
        var contentItem = new ContentItem();

        // Act
        await contentManager.CreateAsync(contentItem, VersionOptions.Draft);

        // Assert
        _contentHandlerMock.Verify(h => h.CreatedAsync(It.IsAny<CreateContentContext>()), Times.Once);
        _contentHandlerMock.Verify(h => h.PublishedAsync(It.IsAny<PublishContentContext>()), Times.Never);
    }

    private DefaultContentManager CreateContentManager() => new DefaultContentManager(
        Mock.Of<IContentDefinitionManager>(),
        Mock.Of<IContentManagerSession>(),
        _contentHandlers,
        Mock.Of<YesSql.ISession>(),
        _contentItemIdGenerator,
        Mock.Of<ILogger<DefaultContentManager>>(),
        Mock.Of<IClock>(),
        Mock.Of<IUpdateModelAccessor>(),
        Mock.Of<IStringLocalizer<DefaultContentManager>>());

    public class TestContentHandler : ContentHandlerBase;
}

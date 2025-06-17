using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Modules;

namespace OrchardCore.ContentManagement.Tests;

public class DefaultContentManagerTests
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultContentManagerTests()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IContentItemIdGenerator, DefaultContentItemIdGenerator>();
        services.AddSingleton<Entities.IIdGenerator, DefaultIdGenerator>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task CreatingContentItem_ShouldNotBePublished_IfVersionNotSpecified()
    {
        // Arrange
        var contentHandlerMock = new Mock<TestContentHandler>();

        var contentManager = new DefaultContentManager(
            Mock.Of<IContentDefinitionManager>(),
            Mock.Of<IContentManagerSession>(),
            new List<IContentHandler>([contentHandlerMock.Object]),
            Mock.Of<YesSql.ISession>(),
            _serviceProvider.GetService<IContentItemIdGenerator>(),
            Mock.Of<ILogger<DefaultContentManager>>(),
            Mock.Of<IClock>(),
            Mock.Of<IUpdateModelAccessor>(),
            Mock.Of<IStringLocalizer<DefaultContentManager>>());

        var contentItem = new ContentItem();

        // Act
        await contentManager.CreateAsync(contentItem);

        // Assert
        contentHandlerMock.Verify(h => h.CreatedAsync(It.IsAny<CreateContentContext>()), Times.Once);

        contentHandlerMock.Verify(h => h.PublishedAsync(It.IsAny<PublishContentContext>()), Times.Never);
    }

    public class TestContentHandler : ContentHandlerBase;
}

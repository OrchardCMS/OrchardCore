using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Modules;

namespace OrchardCore.ContentManagement.Tests;

public class DefaultContentManagerTests : IDisposable
{
    private static bool _created, _published;

    private readonly IServiceProvider _serviceProvider;

    public DefaultContentManagerTests()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IContentItemIdGenerator, DefaultContentItemIdGenerator>();
        services.AddSingleton<Entities.IIdGenerator, DefaultIdGenerator>();

        services.AddScoped<IContentHandler, TestContentHandler>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task CreatingContentItem_ShouldNotBePublished_IfVersionNotSpecified()
    {
        // Arrange
        var contentManager = new DefaultContentManager(
            Mock.Of<IContentDefinitionManager>(),
            Mock.Of<IContentManagerSession>(),
            _serviceProvider.GetServices<IContentHandler>(),
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
        Assert.True(_created);
        Assert.False(_published);
    }

    public void Dispose() => _created = _published = false;

    public class TestContentHandler : ContentHandlerBase
    {
        public override Task CreatedAsync(CreateContentContext context)
        {
            _created = true;

            return Task.CompletedTask;
        }

        public override Task PublishedAsync(PublishContentContext context)
        {
            _published = true;

            return Task.CompletedTask;
        }
    }
}

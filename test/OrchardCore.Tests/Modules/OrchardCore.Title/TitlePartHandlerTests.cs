using Fluid.Values;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.Liquid;
using OrchardCore.Title.Handlers;
using OrchardCore.Title.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Title;

public class TitlePartHandlerTests
{
    private const string ContentType = "Article";
    private const string Pattern = "{{ ContentItem.ContentItemId }}";
    private const string GeneratedTitle = "Generated title";

    [Fact]
    public async Task PublishingAsync_TitleGeneratedFromPattern_SetsDisplayText()
    {
        // Arrange
        var liquidTemplateManager = CreateLiquidTemplateManager();
        var handler = CreateHandler(liquidTemplateManager.Object, TitlePartOptions.GeneratedDisabled);
        var part = CreateTitlePart();

        // Act
        await handler.PublishingAsync(new PublishContentContext(part.ContentItem, null), part);

        // Assert
        Assert.Equal(GeneratedTitle, part.ContentItem.DisplayText);
        Assert.Equal(GeneratedTitle, part.Title);
    }

    [Fact]
    public async Task PublishingAsync_TitleAlreadyGeneratedOnUpdate_DoesNotRegenerateTitle()
    {
        // Arrange
        var liquidTemplateManager = CreateLiquidTemplateManager();
        var handler = CreateHandler(liquidTemplateManager.Object, TitlePartOptions.GeneratedDisabled);
        var part = CreateTitlePart();

        await handler.UpdatedAsync(new UpdateContentContext(part.ContentItem), part);

        // Act
        await handler.PublishingAsync(new PublishContentContext(part.ContentItem, null), part);

        // Assert
        Assert.Equal(GeneratedTitle, part.ContentItem.DisplayText);
        liquidTemplateManager.Verify(
            manager => manager.RenderStringAsync(
                Pattern,
                It.IsAny<TextEncoder>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<KeyValuePair<string, FluidValue>>>()),
            Times.Once());
    }

    [Fact]
    public async Task PublishingAsync_EditableTitle_DoesNotRenderPattern()
    {
        // Arrange
        var liquidTemplateManager = CreateLiquidTemplateManager();
        var handler = CreateHandler(liquidTemplateManager.Object, TitlePartOptions.Editable);
        var part = CreateTitlePart();
        part.Title = "Editor title";

        // Act
        await handler.PublishingAsync(new PublishContentContext(part.ContentItem, null), part);

        // Assert
        Assert.Equal("Editor title", part.ContentItem.DisplayText);
        liquidTemplateManager.Verify(
            manager => manager.RenderStringAsync(
                It.IsAny<string>(),
                It.IsAny<TextEncoder>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<KeyValuePair<string, FluidValue>>>()),
            Times.Never());
    }

    private static Mock<ILiquidTemplateManager> CreateLiquidTemplateManager()
    {
        var liquidTemplateManager = new Mock<ILiquidTemplateManager>();
        liquidTemplateManager
            .Setup(manager => manager.RenderStringAsync(
                Pattern,
                It.IsAny<TextEncoder>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<KeyValuePair<string, FluidValue>>>()))
            .ReturnsAsync(GeneratedTitle);

        return liquidTemplateManager;
    }

    private static TitlePartHandler CreateHandler(ILiquidTemplateManager liquidTemplateManager, TitlePartOptions options)
    {
        var typeDefinition = new ContentTypeDefinitionBuilder()
            .WithName(ContentType)
            .WithPart(nameof(TitlePart), part => part.WithSettings(new TitlePartSettings
            {
                Options = options,
                Pattern = Pattern,
            }))
            .Build();

        var contentDefinitionManager = new Mock<IContentDefinitionManager>();
        contentDefinitionManager
            .Setup(manager => manager.GetTypeDefinitionAsync(ContentType))
            .ReturnsAsync(typeDefinition);

        var localizer = new Mock<IStringLocalizer<TitlePartHandler>>();

        return new TitlePartHandler(liquidTemplateManager, contentDefinitionManager.Object, localizer.Object);
    }

    private static TitlePart CreateTitlePart()
    {
        var contentItem = new ContentItem
        {
            ContentItemId = "item-id",
            ContentType = ContentType,
        };

        var part = contentItem.GetOrCreate<TitlePart>();
        part.ContentItem = contentItem;

        return part;
    }
}

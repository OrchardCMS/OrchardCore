using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Moq;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.Markdown;

public class MarkdownMigrationTests
{
    [Fact]
    public async Task UpdateFrom5_WarnsForLegacyLiquidSettingsWithoutChangingDefinitions()
    {
        var fieldSettings = new JsonObject
        {
            [nameof(MarkdownFieldSettings)] = new JsonObject
            {
                ["RenderLiquid"] = true,
            },
        };
        var field = new ContentPartFieldDefinition(
            new ContentFieldDefinition(nameof(MarkdownField)),
            "Template",
            fieldSettings);
        var namedPart = new ContentPartDefinition("Article", [field], new JsonObject());
        var bodySettings = new JsonObject
        {
            [nameof(MarkdownBodyPartSettings)] = new JsonObject
            {
                ["RenderLiquid"] = true,
            },
        };
        var type = new ContentTypeDefinition(
            "Article",
            "Article",
            [
                new ContentTypePartDefinition(
                    "MarkdownBodyPart",
                    new ContentPartDefinition("MarkdownBodyPart"),
                    bodySettings),
                new ContentTypePartDefinition("Article", namedPart, new JsonObject()),
            ],
            new JsonObject());
        var definitionManager = new Mock<IContentDefinitionManager>();
        definitionManager
            .Setup(manager => manager.LoadTypeDefinitionsAsync())
            .ReturnsAsync([type]);
        var logger = new Mock<ILogger<global::OrchardCore.Markdown.Migrations>>();
        var migration = new global::OrchardCore.Markdown.Migrations(definitionManager.Object, logger.Object);

        var version = await migration.UpdateFrom5Async();

        Assert.Equal(6, version);
        Assert.True(bodySettings[nameof(MarkdownBodyPartSettings)]["RenderLiquid"].GetValue<bool>());
        Assert.True(fieldSettings[nameof(MarkdownFieldSettings)]["RenderLiquid"].GetValue<bool>());
        logger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(2));
        definitionManager.Verify(manager => manager.LoadTypeDefinitionsAsync(), Times.Once);
        definitionManager.VerifyNoOtherCalls();
    }
}

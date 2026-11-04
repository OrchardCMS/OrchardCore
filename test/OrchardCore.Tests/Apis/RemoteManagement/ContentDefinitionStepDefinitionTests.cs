using System.Linq.Expressions;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Deployment;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class ContentDefinitionStepDefinitionTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Selection_AdminAndAdapter_ShareNormalization(bool replace, bool includeAll)
    {
        var updater = new Mock<IUpdateModel>();
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<ContentDefinitionDeploymentStep>(), It.IsAny<string>(), It.IsAny<Expression<Func<ContentDefinitionDeploymentStep, object>>[]>()))
            .Callback((ContentDefinitionDeploymentStep step, string _, Expression<Func<ContentDefinitionDeploymentStep, object>>[] _) =>
            {
                step.IncludeAll = includeAll;
                step.ContentTypes = ["Article", "Article"];
                step.ContentParts = ["TitlePart", "TitlePart"];
            }).ReturnsAsync(true);
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<ReplaceContentDefinitionDeploymentStep>(), It.IsAny<string>(), It.IsAny<Expression<Func<ReplaceContentDefinitionDeploymentStep, object>>[]>()))
            .Callback((ReplaceContentDefinitionDeploymentStep step, string _, Expression<Func<ReplaceContentDefinitionDeploymentStep, object>>[] _) =>
            {
                step.IncludeAll = includeAll;
                step.ContentTypes = ["Article", "Article"];
                step.ContentParts = ["TitlePart", "TitlePart"];
            }).ReturnsAsync(true);
        DeploymentStep admin = replace ? new ReplaceContentDefinitionDeploymentStep() : new ContentDefinitionDeploymentStep();
        if (replace)
        {
            await new ReplaceContentDefinitionDeploymentStepDriver().UpdateAsync((ReplaceContentDefinitionDeploymentStep)admin, Context(updater.Object));
        }
        else
        {
            await new ContentDefinitionDeploymentStepDriver().UpdateAsync((ContentDefinitionDeploymentStep)admin, Context(updater.Object));
        }

        var (definition, candidate) = Create(replace ? nameof(ReplaceContentDefinitionDeploymentStep) : nameof(ContentDefinitionDeploymentStep));
        Assert.Empty(await definition.UpdateAsync(candidate, new JsonObject
        {
            ["includeAll"] = includeAll,
            ["contentTypes"] = new JsonArray("Article", "Article"),
            ["contentParts"] = new JsonArray("TitlePart", "TitlePart"),
        }));
        Assert.True(JsonNode.DeepEquals(definition.Describe(admin), definition.Describe(candidate)));
        Assert.Equal(includeAll ? 0 : 2, definition.Describe(candidate)["contentTypes"].AsArray().Count);
        Assert.Equal(includeAll ? 0 : 1, definition.Describe(candidate)["contentParts"].AsArray().Count);
        var before = definition.Describe(candidate);
        Assert.Empty(await definition.UpdateAsync(candidate, new JsonObject()));
        Assert.True(JsonNode.DeepEquals(before, definition.Describe(candidate)));
    }

    [Theory]
    [InlineData(nameof(ContentDefinitionDeploymentStep))]
    [InlineData(nameof(ReplaceContentDefinitionDeploymentStep))]
    public async Task Update_InvalidReferencesOrShapes_PreserveExistingValues(string type)
    {
        var (definition, step) = Create(type);
        Assert.Empty(await definition.UpdateAsync(step, new JsonObject { ["contentTypes"] = new JsonArray("Article"), ["contentParts"] = new JsonArray("TitlePart") }));
        var before = definition.Describe(step);
        foreach (var patch in new JsonObject[]
        {
            new() { ["contentTypes"] = new JsonArray("Missing") },
            new() { ["contentParts"] = new JsonArray("Missing") },
            new() { ["includeAll"] = true, ["contentTypes"] = null },
            new() { ["includeAll"] = "true" },
            new() { ["contentParts"] = new JsonArray(1) },
            new() { ["contentTypes"] = new JsonArray(" ") },
            new() { ["includeAll"] = true, ["unknown"] = false },
        })
        {
            Assert.NotEmpty(await definition.UpdateAsync(step, patch));
            Assert.True(JsonNode.DeepEquals(before, definition.Describe(step)));
        }
    }

    [Fact]
    public async Task Deletion_AdminAndAdapter_AllowDestinationOnlyNamesAndPreserveDuplicates()
    {
        var updater = new Mock<IUpdateModel>();
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<DeleteContentDefinitionStepViewModel>(), It.IsAny<string>()))
            .Callback((DeleteContentDefinitionStepViewModel model, string _) =>
            {
                model.ContentTypes = "Missing, Missing";
                model.ContentParts = "  Obsolete, ,Obsolete ";
            }).ReturnsAsync(true);
        var admin = new DeleteContentDefinitionDeploymentStep();
        await new DeleteContentDefinitionDeploymentStepDriver().UpdateAsync(admin, Context(updater.Object));
        var (definition, candidate) = Create(nameof(DeleteContentDefinitionDeploymentStep));
        Assert.Empty(await definition.UpdateAsync(candidate, new JsonObject
        {
            ["contentTypes"] = new JsonArray("Missing", "Missing"),
            ["contentParts"] = new JsonArray("Obsolete", "Obsolete"),
        }));
        Assert.True(JsonNode.DeepEquals(definition.Describe(admin), definition.Describe(candidate)));
        Assert.NotEmpty(await definition.UpdateAsync(candidate, new JsonObject { ["includeAll"] = true }));
        Assert.True(JsonNode.DeepEquals(definition.Describe(admin), definition.Describe(candidate)));
    }

    private static (IDeploymentStepDefinition Definition, DeploymentStep Step) Create(string type)
    {
        var definitions = new Mock<IContentDefinitionManager>();
        definitions.Setup(value => value.GetTypeDefinitionAsync("Article")).ReturnsAsync(new ContentTypeDefinition("Article", "Article"));
        definitions.Setup(value => value.GetPartDefinitionAsync("TitlePart")).ReturnsAsync(new ContentPartDefinition("TitlePart"));
        var services = new ServiceCollection();
        new global::OrchardCore.ContentTypes.DeploymentStartup().ConfigureServices(services);
        services.AddSingleton(definitions.Object);
        using var provider = services.BuildServiceProvider();
        var definition = Assert.Single(provider.GetServices<IDeploymentStepDefinition>(), value => value.Type == type);
        var factory = Assert.Single(provider.GetServices<IDeploymentStepFactory>(), value => value.Name == type);
        return (definition, factory.Create());
    }

    private static UpdateEditorContext Context(IUpdateModel updater) =>
        new(new Shape(), "", false, "", Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater);
}

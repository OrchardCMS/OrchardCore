using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.Documents;
using OrchardCore.Json;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Recipes;
using OrchardCore.Layers.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Rules;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.Services;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Modules.OrchardCore.Layers;

public class LayerRecipeTests
{
    [Theory]
    [InlineData(" ", "true")]
    [InlineData("Valid", "function (")]
    public async Task Execute_InvalidDefinition_DoesNotMutateDocument(string name, string script)
    {
        var document = new LayersDocument();
        var documents = new Mock<IDocumentManager<LayersDocument>>();
        documents.Setup(manager => manager.GetOrCreateMutableAsync()).ReturnsAsync(document);
        documents.Setup(manager => manager.GetOrCreateImmutableAsync()).ReturnsAsync(document);
        using var services = CreateServices(documents.Object);
        var context = CreateContext(new JsonObject
        {
            ["Name"] = name,
            ["LayerRule"] = new JsonObject
            {
                ["Conditions"] = new JsonArray(new JsonObject { ["Name"] = "JavascriptCondition", ["Script"] = script }),
            },
        });

        await ActivatorUtilities.CreateInstance<LayerStep>(services).ExecuteAsync(context);

        Assert.NotEmpty(context.Errors);
        Assert.Empty(document.Layers);
        documents.Verify(manager => manager.UpdateAsync(It.IsAny<LayersDocument>()), Times.Never);
    }

    [Fact]
    public async Task Execute_UnknownConditionAfterValidEntry_PreservesExistingDocument()
    {
        var original = new Layer { Name = "Existing", Description = "Original", LayerRule = new Rule { ConditionId = "root" } };
        var document = new LayersDocument();
        document.Layers.Add(original);
        var documents = new Mock<IDocumentManager<LayersDocument>>();
        documents.Setup(manager => manager.GetOrCreateMutableAsync()).ReturnsAsync(document);
        documents.Setup(manager => manager.GetOrCreateImmutableAsync()).ReturnsAsync(document);
        using var services = CreateServices(documents.Object);
        var context = CreateContext(new JsonObject { ["Name"] = "Existing", ["Description"] = "Changed" },
            new JsonObject
            {
                ["Name"] = "New",
                ["LayerRule"] = new JsonObject
                {
                    ["Conditions"] = new JsonArray(new JsonObject { ["Name"] = "UnknownCondition" }),
                },
            });

        await ActivatorUtilities.CreateInstance<LayerStep>(services).ExecuteAsync(context);

        Assert.NotEmpty(context.Errors);
        Assert.Same(original, Assert.Single(document.Layers));
        Assert.Equal("Original", original.Description);
        documents.Verify(manager => manager.UpdateAsync(It.IsAny<LayersDocument>()), Times.Never);
    }

    [Fact]
    public async Task Execute_CreateAndPartialUpdate_PreservesIdentitiesAndReplacesChildren()
    {
        var document = new LayersDocument();
        var documents = new Mock<IDocumentManager<LayersDocument>>();
        documents.Setup(manager => manager.GetOrCreateMutableAsync()).ReturnsAsync(document);
        documents.Setup(manager => manager.GetOrCreateImmutableAsync()).ReturnsAsync(document);
        using var services = CreateServices(documents.Object);
        var handler = ActivatorUtilities.CreateInstance<LayerStep>(services);
        var create = CreateContext(new JsonObject
        {
            ["Name"] = "News", ["Description"] = "Original",
            ["LayerRule"] = new JsonObject
            {
                ["Conditions"] = new JsonArray(new JsonObject
                {
                    ["Name"] = "JavascriptCondition", ["ConditionId"] = "child", ["Script"] = "true",
                }),
            },
        });
        await handler.ExecuteAsync(create);
        Assert.Empty(create.Errors);
        var layer = Assert.Single(document.Layers);
        var rootId = layer.LayerRule.ConditionId;
        Assert.False(string.IsNullOrEmpty(rootId));
        Assert.Equal("child", Assert.Single(layer.LayerRule.Conditions).ConditionId);

        var partial = CreateContext(new JsonObject { ["Name"] = "news", ["Description"] = "" });
        await handler.ExecuteAsync(partial);
        Assert.Empty(partial.Errors);
        Assert.Equal("Original", layer.Description);
        Assert.Equal(rootId, layer.LayerRule.ConditionId);
        Assert.Equal("child", Assert.Single(layer.LayerRule.Conditions).ConditionId);

        var replace = CreateContext(new JsonObject
        {
            ["Name"] = "News", ["Description"] = "Updated",
            ["LayerRule"] = new JsonObject { ["Conditions"] = new JsonArray() },
        });
        await handler.ExecuteAsync(replace);
        Assert.Empty(replace.Errors);
        Assert.Equal("Updated", layer.Description);
        Assert.Equal(rootId, layer.LayerRule.ConditionId);
        Assert.Empty(layer.LayerRule.Conditions);
    }

    [Fact]
    public async Task Execute_RegisteredExtension_PreservesPropertiesOutsideApiContract()
    {
        var document = new LayersDocument();
        var documents = new Mock<IDocumentManager<LayersDocument>>();
        documents.Setup(manager => manager.GetOrCreateMutableAsync()).ReturnsAsync(document);
        documents.Setup(manager => manager.GetOrCreateImmutableAsync()).ReturnsAsync(document);
        using var services = CreateServices(documents.Object);
        var rules = services.GetRequiredService<IRuleManagementService>();
        Assert.False(rules.GetDescriptors().Single(descriptor => descriptor.Name == nameof(ExtensionCondition)).CanWrite);
        var context = CreateContext(new JsonObject
        {
            ["Name"] = "Extension",
            ["LayerRule"] = new JsonObject
            {
                ["ConditionId"] = "root",
                ["Conditions"] = new JsonArray(new JsonObject
                {
                    ["Name"] = nameof(ExtensionCondition), ["ConditionId"] = "custom", ["Value"] = "preserved",
                }),
            },
        });

        await ActivatorUtilities.CreateInstance<LayerStep>(services).ExecuteAsync(context);

        Assert.Empty(context.Errors);
        var rule = Assert.Single(document.Layers).LayerRule;
        Assert.Equal("root", rule.ConditionId);
        var condition = Assert.IsType<ExtensionCondition>(Assert.Single(rule.Conditions));
        Assert.Equal("preserved", condition.Value);
        Assert.Equal("custom", condition.ConditionId);
    }

    [Fact]
    public async Task Execute_LegacyLayerWithoutRule_InitializesRootIdentity()
    {
        var document = new LayersDocument();
        document.Layers.Add(new Layer { Name = "Legacy" });
        var documents = new Mock<IDocumentManager<LayersDocument>>();
        documents.Setup(manager => manager.GetOrCreateMutableAsync()).ReturnsAsync(document);
        documents.Setup(manager => manager.GetOrCreateImmutableAsync()).ReturnsAsync(document);
        using var services = CreateServices(documents.Object);
        var context = CreateContext(new JsonObject { ["Name"] = "Legacy" });

        await ActivatorUtilities.CreateInstance<LayerStep>(services).ExecuteAsync(context);

        Assert.Empty(context.Errors);
        Assert.False(string.IsNullOrEmpty(Assert.Single(document.Layers).LayerRule.ConditionId));
    }

    [Fact]
    public async Task Execute_MissingRootIdentity_DoesNotMutateImmutableDocument()
    {
        var immutable = new LayersDocument();
        immutable.Layers.Add(new Layer { Name = "Legacy", LayerRule = new Rule() });
        var mutable = new LayersDocument();
        mutable.Layers.Add(new Layer { Name = "Legacy", LayerRule = new Rule() });
        var documents = new Mock<IDocumentManager<LayersDocument>>();
        documents.Setup(manager => manager.GetOrCreateMutableAsync()).ReturnsAsync(mutable);
        documents.Setup(manager => manager.GetOrCreateImmutableAsync()).ReturnsAsync(immutable);
        using var services = CreateServices(documents.Object);
        var context = CreateContext(new JsonObject { ["Name"] = "Legacy" });

        await ActivatorUtilities.CreateInstance<LayerStep>(services).ExecuteAsync(context);

        Assert.Empty(context.Errors);
        Assert.Null(Assert.Single(immutable.Layers).LayerRule.ConditionId);
        Assert.False(string.IsNullOrEmpty(Assert.Single(mutable.Layers).LayerRule.ConditionId));
    }

    public sealed class ExtensionCondition : Condition
    {
        public string Value { get; set; }
    }

    private static RecipeExecutionContext CreateContext(params JsonNode[] layers) => new()
    {
        Name = "Layers",
        Step = new JsonObject { ["Layers"] = new JsonArray(layers) },
    };

    private static ServiceProvider CreateServices(IDocumentManager<LayersDocument> documents)
    {
        var ids = new Mock<IConditionIdGenerator>();
        ids.Setup(generator => generator.GenerateUniqueId(It.IsAny<Condition>()))
            .Callback<Condition>(condition => condition.ConditionId = Guid.NewGuid().ToString("N"));
        return new ServiceCollection().AddLogging().AddLocalization()
            .AddSingleton(documents).AddSingleton(Mock.Of<ISession>()).AddSingleton(ids.Object)
            .AddSingleton<IConditionFactory, ConditionFactory<JavascriptCondition>>()
            .AddSingleton<IConditionFactory, ConditionFactory<ExtensionCondition>>()
            .AddSingleton(Options.Create(new DocumentJsonSerializerOptions()))
            .AddSingleton(Options.Create(new ConditionOperatorOptions()))
            .AddSingleton<ILayerService, LayerService>()
            .AddSingleton<IRuleManagementService, RuleManagementService>()
            .BuildServiceProvider();
    }
}

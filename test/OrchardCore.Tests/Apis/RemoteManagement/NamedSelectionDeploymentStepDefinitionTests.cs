using System.Linq.Expressions;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.CustomSettings.Deployment;
using OrchardCore.CustomSettings.Services;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Users.Deployment;
using OrchardCore.Users.Services;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class NamedSelectionDeploymentStepDefinitionTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task AdminAndApi_ShareSelectionNormalization(bool userSettings, bool includeAll)
    {
        var updater = new Mock<IUpdateModel>();
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<CustomSettingsDeploymentStep>(), It.IsAny<string>(), It.IsAny<Expression<Func<CustomSettingsDeploymentStep, object>>[]>()))
            .Callback((CustomSettingsDeploymentStep step, string _, Expression<Func<CustomSettingsDeploymentStep, object>>[] _) =>
            {
                step.IncludeAll = includeAll;
                step.SettingsTypeNames = ["Selected", "Selected"];
            }).ReturnsAsync(true);
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<CustomUserSettingsDeploymentStep>(), It.IsAny<string>()))
            .Callback((CustomUserSettingsDeploymentStep step, string _) =>
            {
                step.IncludeAll = includeAll;
                step.SettingsTypeNames = ["Selected", "Selected"];
            }).ReturnsAsync(true);
        DeploymentStep admin;
        if (userSettings)
        {
            var step = new CustomUserSettingsDeploymentStep();
            await new CustomUserSettingsDeploymentStepDriver(null).UpdateAsync(step, Context(updater.Object));
            admin = step;
        }
        else
        {
            var step = new CustomSettingsDeploymentStep();
            await new CustomSettingsDeploymentStepDriver(null).UpdateAsync(step, Context(updater.Object));
            admin = step;
        }

        var (definition, candidate) = Create(userSettings);
        Assert.Empty(await definition.UpdateAsync(candidate, new JsonObject
        {
            ["includeAll"] = includeAll,
            ["settingsTypeNames"] = new JsonArray("Selected", "Selected"),
        }));
        Assert.True(JsonNode.DeepEquals(definition.Describe(admin), definition.Describe(candidate)));
        Assert.Equal(includeAll ? 0 : 2, definition.Describe(candidate)["settingsTypeNames"].AsArray().Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RegisteredContract_UsesOwningSettingsServiceAndPreservesInvalidOrOmittedValues(bool userSettings)
    {
        var (definition, step) = Create(userSettings);
        Assert.Empty(await definition.UpdateAsync(step, new JsonObject { ["includeAll"] = false, ["settingsTypeNames"] = new JsonArray("Selected") }));
        var before = definition.Describe(step);
        Assert.Empty(await definition.UpdateAsync(step, new JsonObject()));
        Assert.True(JsonNode.DeepEquals(before, definition.Describe(step)));
        foreach (var patch in new JsonObject[]
        {
            new() { ["settingsTypeNames"] = new JsonArray("OtherStereotype") },
            new() { ["settingsTypeNames"] = new JsonArray("missing") },
            new() { ["settingsTypeNames"] = new JsonArray(" ") },
            new() { ["settingsTypeNames"] = null },
            new() { ["includeAll"] = "true" },
            new() { ["includeAll"] = true, ["unknown"] = false },
        })
        {
            Assert.NotEmpty(await definition.UpdateAsync(step, patch));
            Assert.True(JsonNode.DeepEquals(before, definition.Describe(step)));
        }
        Assert.Empty(await definition.UpdateAsync(step, new JsonObject { ["settingsTypeNames"] = new JsonArray() }));
        Assert.Empty(definition.Describe(step)["settingsTypeNames"].AsArray());
    }

    private static (IDeploymentStepDefinition Definition, DeploymentStep Step) Create(bool userSettings)
    {
        var definitions = new Mock<IContentDefinitionManager>();
        definitions.Setup(value => value.ListTypeDefinitionsAsync()).ReturnsAsync(new[]
        {
            new ContentTypeDefinition("Selected", "Selected", [], new JsonObject
            {
                ["ContentTypeSettings"] = new JsonObject { ["Stereotype"] = userSettings ? "CustomUserSettings" : "CustomSettings" },
            }),
            new ContentTypeDefinition("OtherStereotype", "OtherStereotype"),
        });
        var services = new ServiceCollection();
        if (userSettings)
        {
            new global::OrchardCore.Users.CustomUserSettingsStartup().ConfigureServices(services);
            services.AddSingleton(new CustomUserSettingsService(null, definitions.Object, null));
        }
        else
        {
            new global::OrchardCore.CustomSettings.DeploymentStartup().ConfigureServices(services);
            services.AddSingleton(new CustomSettingsService(null, null, null, null, definitions.Object));
        }
        var provider = services.BuildServiceProvider();
        var definition = Assert.Single(provider.GetServices<IDeploymentStepDefinition>());
        var factory = Assert.Single(provider.GetServices<IDeploymentStepFactory>(), value => value.Name == definition.Type);
        return (definition, factory.Create());
    }

    private static UpdateEditorContext Context(IUpdateModel updater) =>
        new(new Shape(), "", false, "", Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater);
}

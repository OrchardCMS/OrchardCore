using System.Text.Json.Nodes;
using OrchardCore.DataLocalization.Deployment;
using OrchardCore.Localization;
using OrchardCore.Localization.Data;

namespace OrchardCore.Tests.Modules.OrchardCore.DataLocalization;

public class TranslationsDeploymentStepDefinitionTests
{
    [Fact]
    public async Task UpdateAsync_ValidatesBeforeMutationAndPreservesOmittedSelection()
    {
        var localization = new Mock<ILocalizationService>();
        localization.Setup(service => service.GetSupportedCulturesAsync()).ReturnsAsync(["en", "fr"]);
        var definition = new TranslationsDeploymentStepDefinition(localization.Object, []);
        var step = new TranslationsDeploymentStep { IncludeAll = false, Cultures = ["fr"] };
        Assert.Empty(await definition.UpdateAsync(step, new JsonObject { ["categories"] = null }));
        Assert.Equal(["fr"], step.Cultures);
        var errors = await definition.UpdateAsync(step, new JsonObject { ["cultures"] = new JsonArray("missing"), ["includeAll"] = false });
        Assert.Contains("cultures", errors.Keys);
        Assert.Equal(["fr"], step.Cultures);
        errors = await definition.UpdateAsync(step, new JsonObject { ["categories"] = new JsonArray("unknown") });
        Assert.Contains("categories", errors.Keys);
        Assert.Empty(step.Categories);
        Assert.Empty(await definition.UpdateAsync(step, new JsonObject { ["cultures"] = new JsonArray("EN") }));
        Assert.Equal(["EN"], step.Cultures);
        Assert.NotEmpty(await definition.UpdateAsync(step, new JsonObject { ["includeAll"] = null }));
        Assert.False(step.IncludeAll);
    }

    [Fact]
    public void SharedSelection_NormalizesNullsWithoutDroppingDormantCultureSelections()
    {
        var step = new TranslationsDeploymentStep();
        TranslationsDeploymentSelection.Apply(step, true, ["fr"], null);
        Assert.True(step.IncludeAll);
        Assert.Equal(["fr"], step.Cultures);
        Assert.Empty(step.Categories);
    }
}

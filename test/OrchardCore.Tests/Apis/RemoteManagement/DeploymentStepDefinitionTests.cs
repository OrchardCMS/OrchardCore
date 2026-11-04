using System.Linq.Expressions;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Deployment;
using OrchardCore.Deployment.Services;
using OrchardCore.Deployment.Steps;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Localization;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class DeploymentStepDefinitionTests
{
    [Theory]
    [InlineData("{\"name\":\"settings\",\"password\":\"example-private-value\"}", true)]
    [InlineData("{\"name\":null}", false)]
    [InlineData("{\"name\":42}", false)]
    [InlineData("{}", false)]
    [InlineData("[]", false)]
    [InlineData("invalid", false)]
    public async Task JsonRecipe_AdminAndContract_ShareValidation(string json, bool valid)
    {
        var current = new JsonRecipeDeploymentStep { Json = "{\"name\":\"original\"}" };
        var updater = Updater();
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<JsonRecipeDeploymentStepViewModel>(), It.IsAny<string>()))
            .Callback((JsonRecipeDeploymentStepViewModel model, string _) => model.Json = json).ReturnsAsync(true);
        var driver = new JsonRecipeDeploymentStepDriver(Localizer<JsonRecipeDeploymentStepDriver>());
        await driver.UpdateAsync(current, Context(updater.Object));
        Assert.Equal(valid, updater.Object.ModelState.IsValid);
        Assert.Equal(valid ? json : "{\"name\":\"original\"}", current.Json);

        var definition = new BuiltInDeploymentStepDefinition(nameof(JsonRecipeDeploymentStep));
        var candidate = new JsonRecipeDeploymentStep { Json = "{\"name\":\"original\"}" };
        Assert.Equal(valid, definition.Update(candidate, new() { ["json"] = json }).Count == 0);
        Assert.Empty(definition.Describe(candidate));
        Assert.True(definition.GetSchema()["properties"]["json"]["writeOnly"].GetValue<bool>());
    }

    [Theory]
    [InlineData("assets/example.txt", true)]
    [InlineData("../outside.txt", false)]
    [InlineData("/outside.txt", false)]
    [InlineData("C:\\outside.txt", false)]
    [InlineData("assets/../outside.txt", false)]
    [InlineData("Recipe.json", false)]
    [InlineData("", false)]
    public async Task CustomFile_AdminAndContract_SharePathValidation(string path, bool valid)
    {
        var current = new CustomFileDeploymentStep { FileName = "original.txt", FileContent = "original" };
        var updater = Updater();
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<CustomFileDeploymentStep>(), It.IsAny<string>(), It.IsAny<Expression<Func<CustomFileDeploymentStep, object>>[]>()))
            .Callback((CustomFileDeploymentStep step, string _, Expression<Func<CustomFileDeploymentStep, object>>[] _) =>
            {
                step.FileName = path;
                step.FileContent = "example-private-value";
            }).ReturnsAsync(true);
        var driver = new CustomFileDeploymentStepDriver(Localizer<CustomFileDeploymentStepDriver>());
        await driver.UpdateAsync(current, Context(updater.Object));
        Assert.Equal(valid, updater.Object.ModelState.IsValid);
        Assert.Equal(valid ? path : "original.txt", current.FileName);
        Assert.Equal(valid ? "example-private-value" : "original", current.FileContent);

        var definition = new BuiltInDeploymentStepDefinition(nameof(CustomFileDeploymentStep));
        var candidate = new CustomFileDeploymentStep();
        Assert.Equal(valid, definition.Update(candidate, new() { ["fileName"] = path, ["fileContent"] = "example-private-value" }).Count == 0);
        Assert.False(definition.Describe(candidate).ContainsKey("fileContent"));
        Assert.True(definition.GetSchema()["properties"]["fileContent"]["writeOnly"].GetValue<bool>());
    }

    [Fact]
    public void Contract_PreservesOmittedValues_AndNormalizesExplicitClear()
    {
        var definition = new BuiltInDeploymentStepDefinition(nameof(CustomFileDeploymentStep));
        var step = new CustomFileDeploymentStep { FileName = "one.txt", FileContent = "keep" };
        Assert.Empty(definition.Update(step, new() { ["fileName"] = "two.txt" }));
        Assert.Equal("keep", step.FileContent);
        Assert.Empty(definition.Update(step, new() { ["fileContent"] = null }));
        Assert.Equal(string.Empty, step.FileContent);
        Assert.Equal("two.txt", step.FileName);
        Assert.NotEmpty(definition.Update(step, new() { ["fileName"] = true }));
        Assert.NotEmpty(definition.Update(step, new() { ["unmanaged"] = "value" }));
        Assert.Equal("two.txt", step.FileName);
    }

    [Fact]
    public void PlanSelection_NormalizesIncludeAll_WithoutExposingOtherData()
    {
        var definition = new BuiltInDeploymentStepDefinition(nameof(DeploymentPlanDeploymentStep));
        var step = new DeploymentPlanDeploymentStep { IncludeAll = false, DeploymentPlanNames = ["Selected"] };
        Assert.Empty(definition.Update(step, new() { ["includeAll"] = true }));
        Assert.Empty(step.DeploymentPlanNames);
        Assert.Empty(definition.Update(step, new() { ["includeAll"] = false, ["deploymentPlanNames"] = new JsonArray("Selected") }));
        Assert.Equal("Selected", Assert.Single(step.DeploymentPlanNames));
        Assert.NotEmpty(definition.Update(step, new() { ["deploymentPlanNames"] = new JsonArray(12) }));
        Assert.Equal("Selected", Assert.Single(step.DeploymentPlanNames));
    }

    [Fact]
    public void RecipeMetadata_UsesExplicitFields_AndPreservesIdentity()
    {
        var definition = new BuiltInDeploymentStepDefinition(nameof(RecipeFileDeploymentStep));
        var step = new RecipeFileDeploymentStep { Id = "step", Author = "Keep" };
        Assert.Empty(definition.Update(step, new() { ["recipeName"] = "Export", ["isSetupRecipe"] = true }));
        Assert.Equal("step", step.Id);
        Assert.Equal("Keep", step.Author);
        Assert.Equal("Export", definition.Describe(step)["recipeName"].GetValue<string>());
        Assert.NotEmpty(definition.Update(step, new() { ["id"] = "replace" }));
        Assert.Equal("step", step.Id);
    }

    private static Mock<IUpdateModel> Updater()
    {
        var updater = new Mock<IUpdateModel>();
        updater.SetupGet(value => value.ModelState).Returns(new ModelStateDictionary());
        return updater;
    }

    private static UpdateEditorContext Context(IUpdateModel updater) =>
        new(new Shape(), "", false, "", Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater);
    private static StringLocalizer<T> Localizer<T>() => new StringLocalizer<T>(new NullStringLocalizerFactory());
}

using System.Linq.Expressions;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Deployment;
using OrchardCore.Contents.Deployment.AddToDeploymentPlan;
using OrchardCore.Contents.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Services;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Environment.Shell;
using OrchardCore.Https.Settings;
using OrchardCore.Localization;
using OrchardCore.Recipes.Models;
using OrchardCore.Settings.Deployment;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class DeploymentContentAdapterTests
{
    [Theory]
    [InlineData("exists", true)]
    [InlineData("missing", false)]
    [InlineData("", false)]
    public async Task SingleContent_AdminAndAdapter_UseSameExistenceValidation(string id, bool valid)
    {
        var content = new Mock<IContentManager>();
        content.Setup(manager => manager.GetAsync("exists", It.IsAny<VersionOptions>())).ReturnsAsync(new ContentItem { ContentItemId = "exists" });
        var updater = Updater();
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<ContentItemDeploymentStepViewModel>(), It.IsAny<string>(), It.IsAny<Expression<Func<ContentItemDeploymentStepViewModel, object>>[]>()))
            .Callback((ContentItemDeploymentStepViewModel model, string _, Expression<Func<ContentItemDeploymentStepViewModel, object>>[] _) => model.ContentItemId = id).ReturnsAsync(true);
        var step = new ContentItemDeploymentStep { Id = "step", ContentItemId = "original" };
        var driver = new ContentItemDeploymentStepDriver(content.Object, new StringLocalizer<ContentItemDeploymentStepDriver>(new NullStringLocalizerFactory()));
        await driver.UpdateAsync(step, Context(updater.Object));
        Assert.Equal(valid, updater.Object.ModelState.IsValid);
        Assert.Equal(valid ? id : "original", step.ContentItemId);
        var candidate = new ContentItemDeploymentStep { Id = "step", ContentItemId = "original" };
        var definition = new ContentItemDeploymentStepDefinition(content.Object);
        Assert.Equal(valid, (await definition.UpdateAsync(candidate, new() { ["contentItemId"] = id })).Count == 0);
        Assert.Equal(step.ContentItemId, candidate.ContentItemId);
        Assert.Equal("step", candidate.Id);
    }

    [Fact]
    public async Task ContentSelection_AdminClearAndApiNull_ShareAssignment()
    {
        var updater = Updater();
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<ContentDeploymentStepViewModel>(), It.IsAny<string>(), It.IsAny<Expression<Func<ContentDeploymentStepViewModel, object>>[]>()))
            .ReturnsAsync(true);
        var step = new ContentDeploymentStep { ContentTypes = ["Old"], ExportAsSetupRecipe = true };
        await new ContentDeploymentStepDriver().UpdateAsync(step, Context(updater.Object));
        Assert.Empty(step.ContentTypes);
        Assert.True(step.ExportAsSetupRecipe);
        var candidate = new ContentDeploymentStep { ContentTypes = ["Old"], ExportAsSetupRecipe = true };
        var definition = new ContentDeploymentStepDefinition(nameof(ContentDeploymentStep));
        Assert.Empty(await definition.UpdateAsync(candidate, new() { ["contentTypes"] = null }));
        Assert.Equal(step.ContentTypes, candidate.ContentTypes);
        Assert.True(candidate.ExportAsSetupRecipe);
        Assert.NotEmpty(await definition.UpdateAsync(candidate, new() { ["contentTypes"] = new JsonArray(12), ["exportAsSetupRecipe"] = false }));
        Assert.True(candidate.ExportAsSetupRecipe);
    }

    [Fact]
    public async Task TenantAdapters_CreateRepresentativeContentAndSettingsPlan_UsingExistingSources()
    {
        using var site = new SiteContext();
        await site.InitializeAsync();
        await site.UsingTenantScopeAsync(async scope =>
        {
            var features = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var available = await features.GetAvailableFeaturesAsync();
            await features.EnableFeaturesAsync(available.Where(feature => feature.Id is "OrchardCore.Deployment" or "OrchardCore.Https" or "OrchardCore.Contents.Deployment.AddToDeploymentPlan"), force: true);
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var registry = scope.ServiceProvider.GetRequiredService<DeploymentStepRegistry>();
            var plans = scope.ServiceProvider.GetRequiredService<IDeploymentPlanService>();
            var content = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var item = await content.NewAsync("BlogPost");
            item.DisplayText = "Deployment adapter sample";
            Assert.True(await content.CreateAsync(item, VersionOptions.Published));
            var settingsType = new SiteSettingsPropertyDeploymentStepFactory<HttpsSettings>().Name;
            var settings = registry.Create(settingsType);
            Assert.NotNull(settings);
            Assert.Empty(await registry.Definition(settings).UpdateAsync(settings, new()));
            Assert.Empty(registry.Definition(settings).Describe(settings));
            Assert.NotEmpty(await registry.Definition(settings).UpdateAsync(settings, new() { ["settings"] = "not a step property" }));

            var selected = registry.Create(nameof(ContentDeploymentStep));
            Assert.Empty(await registry.Definition(selected).UpdateAsync(selected, new() { ["contentTypes"] = new JsonArray("BlogPost"), ["exportAsSetupRecipe"] = false }));
            var single = registry.Create(nameof(ContentItemDeploymentStep));
            Assert.Empty(await registry.Definition(single).UpdateAsync(single, new() { ["contentItemId"] = item.ContentItemId }));
            var all = registry.Create(nameof(AllContentDeploymentStep));
            Assert.Empty(await registry.Definition(all).UpdateAsync(all, new() { ["exportAsSetupRecipe"] = true }));
            Assert.NotEmpty(await registry.Definition(all).UpdateAsync(all, new() { ["contentTypes"] = new JsonArray("BlogPost") }));
            var plan = (await plans.CreateAsync("Representative export")).Plan;
            Assert.True((await plans.AddStepsAsync(plan.Id, [settings, selected, single])).Changed);
            Assert.All(plan.DeploymentSteps, step => Assert.False(string.IsNullOrWhiteSpace(step.Id)));

            var sources = scope.ServiceProvider.GetServices<IDeploymentSource>().ToArray();
            var result = new DeploymentPlanResult(Mock.Of<IFileBuilder>(), new RecipeDescriptor { Name = "test" });
            await sources.OfType<SiteSettingsPropertyDeploymentSource<HttpsSettings>>().Single().ProcessDeploymentStepAsync(settings, result);
            await sources.OfType<ContentDeploymentSource>().Single().ProcessDeploymentStepAsync(selected, result);
            Assert.Contains(result.Steps, step => step.ContainsKey(nameof(HttpsSettings)));
            var exported = Assert.Single(result.Steps, step => string.Equals(step["name"]?.GetValue<string>(), "content", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(exported["data"].AsArray(), entry => entry[nameof(ContentItem.ContentItemId)]?.GetValue<string>() == item.ContentItemId);
        });
    }

    private static Mock<IUpdateModel> Updater()
    {
        var updater = new Mock<IUpdateModel>();
        updater.SetupGet(value => value.ModelState).Returns(new ModelStateDictionary());
        return updater;
    }
    private static UpdateEditorContext Context(IUpdateModel updater) =>
        new(new Shape(), "", false, "", Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater);
}

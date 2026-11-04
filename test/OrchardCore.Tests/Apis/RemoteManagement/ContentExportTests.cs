using System.Security.Claims;
using System.Linq.Expressions;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Deployment;
using OrchardCore.Contents.Deployment.Download;
using OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class ContentExportTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DownloadAndQueuedExport_ShareVersionAndSerialization(bool latest)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity("probe"));
        var item = new ContentItem { ContentItemId = "selected", ContentType = "Article", Id = 123, DisplayText = "exported" };
        var content = new Mock<IContentManager>();
        content.Setup(value => value.GetAsync("selected", latest ? VersionOptions.Latest : VersionOptions.Published)).ReturnsAsync(item);
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.AuthorizeAsync(user, It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success());
        var exports = new ContentExportService(content.Object, authorization.Object);
        var controller = new DownloadController(authorization.Object, content.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } },
        };
        var download = Assert.IsType<FileContentResult>(await controller.Download("selected", latest));
        Assert.True(JsonNode.DeepEquals(ContentExportService.Serialize(item), JsonNode.Parse(download.FileContents)));
        var source = new ExportContentToDeploymentTargetDeploymentSource(Mock.Of<YesSql.ISession>(), Mock.Of<IUpdateModelAccessor>(), exports, new HttpContextAccessor());
        var result = new DeploymentPlanResult(Mock.Of<IFileBuilder>(), new RecipeDescriptor()) { User = user };
        await source.ProcessDeploymentStepAsync(new ExportContentToDeploymentTargetDeploymentStep { ContentItemIds = ["selected"], Latest = latest }, result);
        var exported = Assert.Single(result.Steps)["data"].AsArray()[0].AsObject();
        Assert.False(exported.ContainsKey("Id"));
        Assert.Equal("selected", exported["ContentItemId"].GetValue<string>());
        Assert.True(JsonNode.DeepEquals(ContentExportService.Serialize(item, recipe: true), exported));
    }

    [Fact]
    public async Task Export_DeniedItem_FailsBeforeAddingRecipeData()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity("probe"));
        var item = new ContentItem { ContentItemId = "denied" };
        var content = new Mock<IContentManager>();
        content.Setup(value => value.GetAsync("denied", VersionOptions.Published)).ReturnsAsync(item);
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.AuthorizeAsync(user, null, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success());
        authorization.Setup(value => value.AuthorizeAsync(user, item, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Failed());
        var exports = new ContentExportService(content.Object, authorization.Object);
        var source = new ExportContentToDeploymentTargetDeploymentSource(Mock.Of<YesSql.ISession>(), Mock.Of<IUpdateModelAccessor>(), exports, new HttpContextAccessor());
        var result = new DeploymentPlanResult(Mock.Of<IFileBuilder>(), new RecipeDescriptor()) { User = user };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => source.ProcessDeploymentStepAsync(new ExportContentToDeploymentTargetDeploymentStep { ContentItemIds = ["denied"] }, result));
        Assert.Empty(result.Steps);
    }

    [Fact]
    public async Task AdminFormSelection_UsesSharedExportAndLatestVersion()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity("admin"));
        var item = new ContentItem { ContentItemId = "selected" };
        var content = new Mock<IContentManager>();
        content.Setup(value => value.GetAsync("selected", VersionOptions.Latest)).ReturnsAsync(item);
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.AuthorizeAsync(user, It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(AuthorizationResult.Success());
        var updater = new Mock<IUpdateModel>();
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<ExportContentToDeploymentTargetDeploymentSource.ExportContentToDeploymentTargetModel>(),
            "ExportContentToDeploymentTarget", It.IsAny<Expression<Func<ExportContentToDeploymentTargetDeploymentSource.ExportContentToDeploymentTargetModel, object>>[]>()))
            .Callback((ExportContentToDeploymentTargetDeploymentSource.ExportContentToDeploymentTargetModel model, string _, Expression<Func<ExportContentToDeploymentTargetDeploymentSource.ExportContentToDeploymentTargetModel, object>>[] _) =>
            {
                model.ContentItemId = "selected";
                model.Latest = true;
            }).ReturnsAsync(true);
        var accessor = new Mock<IUpdateModelAccessor>();
        accessor.SetupGet(value => value.ModelUpdater).Returns(updater.Object);
        var source = new ExportContentToDeploymentTargetDeploymentSource(Mock.Of<YesSql.ISession>(), accessor.Object,
            new ContentExportService(content.Object, authorization.Object), new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = user } });
        var result = new DeploymentPlanResult(Mock.Of<IFileBuilder>(), new RecipeDescriptor());
        await source.ProcessDeploymentStepAsync(new ExportContentToDeploymentTargetDeploymentStep(), result);
        Assert.Equal("selected", Assert.Single(result.Steps)["data"][0]["ContentItemId"].GetValue<string>());
    }

    [Fact]
    public async Task Selection_InvalidOrMissingVersions_PreservesStoredStep()
    {
        var content = new Mock<IContentManager>();
        content.Setup(value => value.GetAsync("selected", VersionOptions.Published)).ReturnsAsync(new ContentItem { ContentItemId = "selected" });
        var services = new ServiceCollection();
        new ExportContentToDeploymentTargetStartup().ConfigureServices(services);
        services.AddSingleton(content.Object);
        using var provider = services.BuildServiceProvider();
        var definition = Assert.Single(provider.GetServices<IDeploymentStepDefinition>(), value => value.Type == nameof(ExportContentToDeploymentTargetDeploymentStep));
        var step = new ExportContentToDeploymentTargetDeploymentStep();
        Assert.NotEmpty(await definition.UpdateAsync(step, new JsonObject()));
        Assert.Empty(await definition.UpdateAsync(step, new JsonObject { ["contentItemIds"] = new JsonArray("selected", "selected") }));
        var before = definition.Describe(step);
        foreach (var patch in new JsonObject[]
        {
            new() { ["contentItemIds"] = new JsonArray() },
            new() { ["contentItemIds"] = new JsonArray("missing") },
            new() { ["latest"] = true },
            new() { ["contentItemIds"] = null },
            new() { ["unexpected"] = true },
        })
        {
            Assert.NotEmpty(await definition.UpdateAsync(step, patch));
            Assert.True(JsonNode.DeepEquals(before, definition.Describe(step)));
        }
        Assert.Single(step.ContentItemIds);
    }
}

using System.Linq.Expressions;
using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using OrchardCore.Media.Deployment;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class MediaDeploymentStepDefinitionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Selection_AdminAndApi_ClearSelectionsWhenIncludingAll(bool includeAll)
    {
        var files = Files();
        var updater = new Mock<IUpdateModel>();
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<MediaDeploymentStep>(), It.IsAny<string>(), It.IsAny<Expression<Func<MediaDeploymentStep, object>>[]>()))
            .Callback((MediaDeploymentStep step, string _, Expression<Func<MediaDeploymentStep, object>>[] _) =>
            {
                step.IncludeAll = includeAll;
                step.FilePaths = ["images/logo.svg"];
                step.DirectoryPaths = ["images"];
            }).ReturnsAsync(true);
        var admin = new MediaDeploymentStep();
        await new MediaDeploymentStepDriver(files.Object).UpdateAsync(admin,
            new UpdateEditorContext(new Shape(), "", false, "", Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater.Object));
        var candidate = new MediaDeploymentStep();
        var definition = new MediaDeploymentStepDefinition(files.Object);
        Assert.Empty(await definition.UpdateAsync(candidate, new JsonObject
        {
            ["includeAll"] = includeAll,
            ["filePaths"] = new JsonArray("images/logo.svg"),
            ["directoryPaths"] = new JsonArray("images"),
        }));
        Assert.Equal(admin.FilePaths, candidate.FilePaths);
        Assert.Equal(admin.DirectoryPaths, candidate.DirectoryPaths);
        Assert.Equal(includeAll, candidate.IncludeAll);
        Assert.Equal(includeAll ? 0 : 1, candidate.FilePaths.Length);
    }

    [Theory]
    [InlineData("../outside.svg")]
    [InlineData("/absolute.svg")]
    [InlineData("images/../outside.svg")]
    [InlineData("images\\logo.svg")]
    [InlineData("https://example.test/image.svg")]
    [InlineData("images/missing.svg")]
    public async Task InvalidPath_PreservesExistingSelection(string invalid)
    {
        var definition = new MediaDeploymentStepDefinition(Files().Object);
        var step = new MediaDeploymentStep { IncludeAll = false, FilePaths = ["images/logo.svg"], DirectoryPaths = ["images"] };
        var before = definition.Describe(step);
        Assert.NotEmpty(await definition.UpdateAsync(step, new JsonObject { ["filePaths"] = new JsonArray(invalid), ["directoryPaths"] = new JsonArray() }));
        Assert.True(JsonNode.DeepEquals(before, definition.Describe(step)));
        Assert.Empty(await definition.UpdateAsync(step, new JsonObject()));
        Assert.True(JsonNode.DeepEquals(before, definition.Describe(step)));
    }

    [Fact]
    public async Task RegistrationAndShapeValidation_UseExplicitMediaContract()
    {
        var services = new ServiceCollection();
        new global::OrchardCore.Media.DeploymentStartup().ConfigureServices(services);
        services.AddSingleton(Files().Object);
        using var provider = services.BuildServiceProvider();
        var definition = Assert.Single(provider.GetServices<IDeploymentStepDefinition>(), value => value.Type == nameof(MediaDeploymentStep));
        var factory = Assert.Single(provider.GetServices<IDeploymentStepFactory>(), value => value.Name == definition.Type);
        var step = factory.Create();
        var before = definition.Describe(step);
        foreach (var patch in new JsonObject[]
        {
            new() { ["includeAll"] = "true" },
            new() { ["filePaths"] = null },
            new() { ["directoryPaths"] = new JsonArray(5) },
            new() { ["includeAll"] = false, ["unknown"] = true },
            new() { ["includeAll"] = false, ["directoryPaths"] = new JsonArray("missing") },
        })
        {
            Assert.NotEmpty(await definition.UpdateAsync(step, patch));
            Assert.True(JsonNode.DeepEquals(before, definition.Describe(step)));
        }
    }

    private static Mock<IMediaFileStore> Files()
    {
        var files = new Mock<IMediaFileStore>();
        files.Setup(value => value.GetFileInfoAsync("images/logo.svg")).ReturnsAsync(Mock.Of<IFileStoreEntry>());
        files.Setup(value => value.GetDirectoryInfoAsync("images")).ReturnsAsync(Mock.Of<IFileStoreEntry>());
        return files;
    }
}

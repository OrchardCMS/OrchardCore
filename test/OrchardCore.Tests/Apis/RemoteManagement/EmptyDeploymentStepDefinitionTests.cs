using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Steps;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class EmptyDeploymentStepDefinitionTests
{
    [Fact]
    public async Task EmptyContract_RejectsHiddenPropertiesAndOtherStepTypes()
    {
        var definition = new EmptyDeploymentStepDefinition<CustomFileDeploymentStep>(nameof(CustomFileDeploymentStep));
        var step = new CustomFileDeploymentStep { Id = "stable", FileContent = "private" };
        Assert.Empty(definition.Describe(step));
        Assert.Empty(await definition.UpdateAsync(step, []));
        Assert.NotEmpty(await definition.UpdateAsync(step, new JsonObject { ["fileContent"] = "replace" }));
        Assert.NotEmpty(await definition.UpdateAsync(step, null));
        Assert.Equal("private", step.FileContent);
        Assert.Throws<ArgumentException>(() => definition.Describe(new JsonRecipeDeploymentStep()));
    }
}

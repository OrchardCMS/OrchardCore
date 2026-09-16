using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Features.Deployment;
using OrchardCore.Templates.Deployment;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class BooleanDeploymentStepDefinitionTests
{
    [Theory]
    [InlineData("features")]
    [InlineData("templates")]
    [InlineData("admin-templates")]
    public async Task ExplicitSwitch_PreservesOmissionsAndRejectsInvalidPatchesBeforeMutation(string kind)
    {
        var services = new ServiceCollection();
        DeploymentStep step;
        string property;
        if (kind == "features")
        {
            new global::OrchardCore.Features.Startup().ConfigureServices(services);
            step = new AllFeaturesDeploymentStep(); property = "ignoreDisabledFeatures";
        }
        else if (kind == "templates")
        {
            new global::OrchardCore.Templates.Startup().ConfigureServices(services);
            step = new AllTemplatesDeploymentStep(); property = "exportAsFiles";
        }
        else
        {
            new global::OrchardCore.Templates.AdminTemplatesStartup().ConfigureServices(services);
            step = new AllAdminTemplatesDeploymentStep(); property = "exportAsFiles";
        }
        var descriptor = Assert.Single(services, item => item.ServiceType == typeof(IDeploymentStepDefinition));
        var definition = Assert.IsAssignableFrom<IDeploymentStepDefinition>(descriptor.ImplementationInstance);
        using var provider = services.BuildServiceProvider();
        var factory = Assert.Single(provider.GetServices<IDeploymentStepFactory>());
        Assert.Equal(factory.Name, definition.Type);
        Assert.Equal(step.GetType(), factory.Create().GetType());
        Assert.Empty(await definition.UpdateAsync(step, new JsonObject { [property] = true }));
        Assert.Empty(await definition.UpdateAsync(step, new JsonObject()));
        Assert.True(definition.Describe(step)[property].GetValue<bool>());
        foreach (var invalid in new JsonObject[] { new() { [property] = "false" }, new() { [property] = null }, new() { [property] = false, ["unknown"] = true } })
        {
            Assert.NotEmpty(await definition.UpdateAsync(step, invalid));
            Assert.True(definition.Describe(step)[property].GetValue<bool>());
        }
        Assert.Empty(await definition.UpdateAsync(step, new JsonObject { [property] = false }));
        Assert.False(definition.Describe(step)[property].GetValue<bool>());
        Assert.Single(definition.GetSchema()["properties"].AsObject());
    }
}

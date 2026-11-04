using System.Text.Json.Nodes;
using OrchardCore.Settings;
using OrchardCore.Settings.Deployment;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class SiteSettingsDeploymentStepDefinitionTests
{
    [Fact]
    public async Task Selection_UsesExporterNamesRejectsUnknownAndNormalizesDuplicates()
    {
        var definition = new SiteSettingsDeploymentStepDefinition();
        var step = new SiteSettingsDeploymentStep { Settings = ["SiteName"] };
        Assert.NotEmpty(await definition.UpdateAsync(step, new JsonObject { ["settings"] = new JsonArray("Properties") }));
        Assert.Equal(["SiteName"], step.Settings);
        Assert.Empty(await definition.UpdateAsync(step, new JsonObject { ["settings"] = new JsonArray("SiteName", "SiteName", "BaseUrl") }));
        Assert.Equal(["SiteName", "BaseUrl"], step.Settings);
        var site = new Mock<ISite>();
        site.SetupGet(site => site.SiteName).Returns("Managed");
        site.SetupGet(site => site.BaseUrl).Returns("https://example.test");
        var export = SiteSettingsDeploymentSelection.Export(site.Object, step.Settings);
        Assert.Equal("Managed", export["SiteName"].GetValue<string>());
        Assert.Equal("https://example.test", export["BaseUrl"].GetValue<string>());
        Assert.DoesNotContain("Properties", SiteSettingsDeploymentSelection.Names);
    }
}

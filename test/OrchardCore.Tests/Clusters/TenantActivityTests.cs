using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tests.Clusters;

public class TenantActivityTests
{
    [Fact]
    public void Update_NoShellContext_IsAsleep()
    {
        var model = new ShellSettingsEntryActivityViewModel();

        model.Update(null, "node");

        Assert.False(model.IsAwake);
        Assert.Null(model.LastRequestUtc);
        Assert.Equal("node", model.NodeName);
    }

    [Fact]
    public void Update_NoPipeline_IsAsleep()
    {
        var model = new ShellSettingsEntryActivityViewModel();

        model.Update(new ShellContext { Settings = new ShellSettings { Name = "Tenant" } }, "node");

        Assert.False(model.IsAwake);
        Assert.Null(model.LastRequestUtc);
    }

    [Fact]
    public void Update_Pipeline_IsAwakeWithLastRequest()
    {
        var lastRequestUtc = DateTime.UtcNow.AddMinutes(-5);
        var model = new ShellSettingsEntryActivityViewModel();

        model.Update(new ShellContext
        {
            Settings = new ShellSettings { Name = "Tenant" },
            Pipeline = Mock.Of<IShellPipeline>(),
            LastRequestTimeUtc = lastRequestUtc,
        }, "node");

        Assert.True(model.IsAwake);
        Assert.Equal(lastRequestUtc, model.LastRequestUtc);
    }
}

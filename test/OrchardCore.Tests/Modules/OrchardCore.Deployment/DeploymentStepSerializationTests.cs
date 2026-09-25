using System.Text.Json;
using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Tests.Modules.OrchardCore.Deployment;

public class DeploymentStepSerializationTests
{
    [Fact]
    public void DeploymentStep_RoundTrips_WhenItsLocalizedNamesWereSet()
    {
        // A step is built through its localizer constructor when the user adds it to a plan, so both localized names
        // are set on the instance that gets persisted. LocalizedString has no constructor the serializer can call, so
        // persisting either of them makes the plan unreadable from that point on.
        var step = new StubDeploymentStep
        {
            Category = new LocalizedString("Content", "Content"),
            Title = new LocalizedString("All Content", "All Content"),
        };

        var json = JsonSerializer.Serialize(step, JOptions.Base);

        var deserialized = JsonSerializer.Deserialize<StubDeploymentStep>(json, JOptions.Base);

        Assert.Equal("Stub", deserialized.Name);
    }

    [Fact]
    public void DeploymentStep_DoesNotPersistItsLocalizedNames()
    {
        // They are read from a fresh instance built by the step's factory, never from the stored plan, so writing them
        // would only be dead data.
        var step = new StubDeploymentStep
        {
            Category = new LocalizedString("Content", "Content"),
            Title = new LocalizedString("All Content", "All Content"),
        };

        var json = JsonSerializer.Serialize(step, JOptions.Base);

        Assert.DoesNotContain("Category", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Title", json, StringComparison.Ordinal);
    }

    private sealed class StubDeploymentStep : DeploymentStep
    {
        public StubDeploymentStep()
        {
            Name = "Stub";
        }
    }
}

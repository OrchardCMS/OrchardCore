using System.Text.Json;
using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Tests.Modules.OrchardCore.Deployment;

public class DeploymentStepSerializationTests
{
    [Fact]
    public void DeploymentStep_WithLocalizedNames_RoundTrips()
    {
        var step = new StubDeploymentStep();

        var json = JsonSerializer.Serialize(step, JOptions.Base);

        var deserialized = JsonSerializer.Deserialize<StubDeploymentStep>(json, JOptions.Base);

        Assert.Equal("Stub", deserialized.Name);
    }

    [Fact]
    public void DeploymentStep_DoesNotPersistItsLocalizedNames()
    {
        // They are read from a fresh instance built by the step's factory, never from the stored plan, so writing them
        // would only be dead data.
        var step = new StubDeploymentStep();

        var json = JsonSerializer.Serialize(step, JOptions.Base);

        Assert.DoesNotContain("Category", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Title", json, StringComparison.Ordinal);
    }

    private sealed class StubDeploymentStep : DeploymentStep
    {
        [JsonIgnore]
        public override LocalizationSource Category => CommonLocalizationSources.Content;

        [JsonIgnore]
        public override LocalizationSource Title => LocalizationSource.Create<StubDeploymentStep>("All Content");

        public StubDeploymentStep()
        {
            Name = "Stub";
        }
    }
}

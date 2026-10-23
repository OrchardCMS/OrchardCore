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
    public void DeploymentStep_WithName_PersistsItsName()
    {
        // Act
        var json = JsonSerializer.Serialize(new StubDeploymentStep(), JOptions.Default);

        // Assert
        Assert.Contains("\"Name\":\"Stub\"", json, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Stub")]
    [InlineData("Renamed")]
    public void DeploymentStep_StoredStep_ReadsItsNameFromItsType(string storedName)
    {
        // Arrange
        var json = $$"""{"Id":"1","Name":"{{storedName}}","Value":"Data"}""";

        // Act
        var deserialized = JsonSerializer.Deserialize<StubDeploymentStep>(json, JOptions.Default);

        // Assert
        Assert.Equal("Stub", deserialized.Name);
        Assert.Equal("1", deserialized.Id);
        Assert.Equal("Data", deserialized.Value);
    }

    [Fact]
    public void UnknownDeploymentStep_StoredStep_KeepsItsStoredName()
    {
        // Arrange
        var json = """{"Id":"1","Name":"MissingStep"}""";

        // Act
        var deserialized = JsonSerializer.Deserialize<UnknownDeploymentStep>(json, JOptions.Default);

        // Assert
        Assert.Equal("MissingStep", deserialized.Name);
    }

    [Fact]
    public void DeploymentStep_DoesNotPersistItsLocalizedNames()
    {
        // They are read from the step's type, never from the stored plan, so writing them
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

        public override string Name => "Stub";

        public string Value { get; set; }
    }
}

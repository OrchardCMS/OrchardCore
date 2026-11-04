using OrchardCore.RemoteManagement;

namespace OrchardCore.Cli.Tests;

public class CompatibilityServiceTests
{
    [Fact]
    public void Evaluate_WhenManifestIsCompatible_ReturnsPositiveFlags()
    {
        var manifest = new RemoteManagementManifest
        {
            ProtocolMajorVersion = RemoteManagementConstants.ProtocolMajorVersion,
            MinimumCliVersion = "1.0.0",
            RecommendedCliVersion = "1.0.0",
        };

        manifest.Capabilities.Add(new RemoteManagementCapability
        {
            Id = "remote-management",
            Version = "1.0.0",
        });

        var result = CompatibilityService.Evaluate(manifest);

        Assert.True(result.ProtocolCompatible);
        Assert.True(result.MinimumVersionSatisfied);
        Assert.True(result.RecommendedVersionSatisfied);
        Assert.True(Assert.Single(result.Capabilities).MajorCompatible);
    }

    [Fact]
    public void Evaluate_WhenMinimumVersionIsHigher_ReturnsNegativeFlag()
    {
        var manifest = new RemoteManagementManifest
        {
            ProtocolMajorVersion = RemoteManagementConstants.ProtocolMajorVersion,
            MinimumCliVersion = "9.9.9",
        };

        var result = CompatibilityService.Evaluate(manifest);

        Assert.False(result.MinimumVersionSatisfied);
    }

    [Fact]
    public void Evaluate_WhenServerMinorIsNewer_ReturnsCompatibleWarningFlag()
    {
        var manifest = new RemoteManagementManifest
        {
            ProtocolMajorVersion = RemoteManagementConstants.ProtocolMajorVersion,
            ProtocolMinorVersion = RemoteManagementConstants.ProtocolMinorVersion + 1,
        };

        var result = CompatibilityService.Evaluate(manifest);

        Assert.True(result.ProtocolCompatible);
        Assert.True(result.ServerUsesNewerMinor);
        Assert.Equal(RemoteManagementConstants.ProtocolMinorVersion + 1, result.ManifestProtocolMinor);
    }
}

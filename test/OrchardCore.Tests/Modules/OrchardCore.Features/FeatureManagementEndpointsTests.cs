using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Features.Endpoints.Management;
using OrchardCore.Features.Models;
using OrchardCore.Modules.Manifest;

namespace OrchardCore.Tests.Modules.OrchardCore.Features;

public class FeatureManagementEndpointsTests
{
    [Fact]
    public void ToResponse_MapsFeatureMetadataAndSortsRelationships()
    {
        var dependencyB = new TestFeatureInfo("Feature.B", "Feature B");
        var dependencyA = new TestFeatureInfo("Feature.A", "Feature A");
        var dependentB = new TestFeatureInfo("Feature.D", "Feature D");
        var dependentA = new TestFeatureInfo("Feature.C", "Feature C");

        var response = FeatureManagementEndpoints.ToResponse(new ModuleFeature
        {
            Descriptor = new TestFeatureInfo("Feature.Main", "Main Feature", category: "Content", description: "Desc", defaultTenantOnly: true),
            IsEnabled = true,
            IsAlwaysEnabled = false,
            EnabledByDependencyOnly = true,
            FeatureDependencies = [dependencyB, dependencyA],
            EnabledDependentFeatures = [dependentB, dependentA],
        });

        Assert.Equal("Feature.Main", response.Id);
        Assert.Equal("Main Feature", response.Name);
        Assert.Equal("Content", response.Category);
        Assert.True(response.IsEnabled);
        Assert.True(response.EnabledByDependencyOnly);
        Assert.True(response.DefaultTenantOnly);
        Assert.Equal(["Feature.A", "Feature.B"], response.Dependencies);
        Assert.Equal(["Feature.C", "Feature.D"], response.EnabledDependents);
        Assert.Equal("Test.Module", response.ExtensionId);
        Assert.Equal("Test Module", response.ExtensionName);
    }

    private sealed class TestFeatureInfo(
        string id,
        string name,
        string category = "General",
        string description = null,
        bool defaultTenantOnly = false) : IFeatureInfo
    {
        public string Id { get; } = id;
        public string Name { get; } = name;
        public int Priority => 0;
        public string Category { get; } = category;
        public string Description { get; } = description;
        public bool DefaultTenantOnly { get; } = defaultTenantOnly;
        public IExtensionInfo Extension { get; } = new TestExtensionInfo();
        public string[] Before => [];
        public string[] After => [];
        public string[] Dependencies => [];
        public bool IsAlwaysEnabled => false;
        public bool EnabledByDependencyOnly => false;
    }

    private sealed class TestExtensionInfo : IExtensionInfo
    {
        public string Id => "Test.Module";
        public string SubPath => "Modules/Test.Module";
        public IManifestInfo Manifest { get; } = new TestManifestInfo();
        public IEnumerable<IFeatureInfo> Features => [];
        public bool Exists => true;
    }

    private sealed class TestManifestInfo : IManifestInfo
    {
        public bool Exists => true;
        public string Name => "Test Module";
        public string Description => null;
        public string Type => "Module";
        public string Author => null;
        public string Website => null;
        public string Version => "1.0.0";
        public IEnumerable<string> Tags => [];
        public ModuleAttribute ModuleInfo => null;
    }
}

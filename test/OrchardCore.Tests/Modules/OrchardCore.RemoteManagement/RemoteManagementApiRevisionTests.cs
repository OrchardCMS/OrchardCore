using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Tests.Modules.OrchardCore.RemoteManagement;

public class RemoteManagementApiRevisionTests
{
    [Fact]
    public void Constructor_FeaturesAndModulesReordered_ProducesSameRevision()
    {
        var first = new RemoteManagementApiRevision(Blueprint(["A", "B"], [typeof(Startup), typeof(string)]));
        var second = new RemoteManagementApiRevision(Blueprint(["B", "A", "A"], [typeof(string), typeof(Startup)]));

        Assert.Equal(first.Value, second.Value);
        Assert.Matches("^[0-9a-f]{64}$", first.Value);
    }

    [Fact]
    public void Constructor_FeaturesOrModulesChanged_ProducesDifferentRevision()
    {
        var original = new RemoteManagementApiRevision(Blueprint(["A"], [typeof(Startup)]));

        Assert.NotEqual(original.Value, new RemoteManagementApiRevision(Blueprint(["A", "B"], [typeof(Startup)])).Value);
        Assert.NotEqual(original.Value, new RemoteManagementApiRevision(Blueprint(["A"], [typeof(Startup), typeof(string)])).Value);
    }

    [Fact]
    public void Value_DescriptorMutated_RemainsBoundToOriginalPipeline()
    {
        var blueprint = Blueprint(["A"], [typeof(Startup)]);
        var revision = new RemoteManagementApiRevision(blueprint);
        var original = revision.Value;
        blueprint.Descriptor.Features.Add(new ShellFeature("B"));

        Assert.Equal(original, revision.Value);
        Assert.NotEqual(original, new RemoteManagementApiRevision(blueprint).Value);
    }

    private static ShellBlueprint Blueprint(string[] features, Type[] types) => new()
    {
        Descriptor = new ShellDescriptor { Features = features.Select(id => new ShellFeature(id)).ToList() },
        Dependencies = types.ToDictionary(type => type, _ => Enumerable.Empty<IFeatureInfo>()),
    };
}

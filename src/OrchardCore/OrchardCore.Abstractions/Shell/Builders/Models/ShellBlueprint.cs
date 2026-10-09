using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Environment.Shell.Builders.Models;

/// <summary>
/// Contains the information necessary to initialize an IoC container
/// for a particular tenant. This model is created by the ICompositionStrategy
/// and is passed into the IShellContainerFactory.
/// </summary>
public class ShellBlueprint
{
    public ShellSettings Settings { get; set; } = null!;
    public ShellDescriptor Descriptor { get; set; } = null!;

    public IDictionary<Type, IEnumerable<IFeatureInfo>> Dependencies { get; set; } = null!;
}

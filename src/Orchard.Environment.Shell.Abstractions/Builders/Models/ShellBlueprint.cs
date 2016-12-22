using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell.Descriptor.Models;

namespace Orchard.Environment.Shell.Builders.Models
{
    /// <summary>
    /// Contains the information necessary to initialize an IoC container
    /// for a particular tenant. This model is created by the ICompositionStrategy
    /// and is passed into the IShellContainerFactory.
    /// </summary>
    public class ShellBlueprint
    {
        private static readonly IFeatureInfo CoreFeature
            = new InternalFeatureInfo("Core", new InternalExtensionInfo("Core"));

        private readonly ShellSettings _shellSettings;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IDictionary<Type, DependencyBlueprint> _dependencies;

        public ShellBlueprint(
            ShellSettings shellSettings,
            ShellDescriptor shellDescriptor,
            IDictionary<Type, DependencyBlueprint> dependencies)
        {
            _shellSettings = shellSettings;
            _shellDescriptor = shellDescriptor;
            _dependencies = dependencies;
        }

        public ShellSettings Settings { get { return _shellSettings; } }
        public ShellDescriptor Descriptor { get { return _shellDescriptor; } }
        public IDictionary<Type, DependencyBlueprint> Dependencies { get { return _dependencies; } }

        public IFeatureInfo GetFeatureForDependency(Type dependency)
        {
            if (Dependencies.ContainsKey(dependency))
            {
                return Dependencies[dependency].Feature;
            }

            return CoreFeature;
        }
    }

    public class ShellBlueprintItem
    {
        public Type Type { get; set; }
        public IFeatureInfo Feature { get; set; }
    }

    public class DependencyBlueprint : ShellBlueprintItem
    {
        public IEnumerable<ShellParameter> Parameters { get; set; }
    }
}
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell.Descriptor.Models;
using System;
using System.Collections.Generic;

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

        public ShellSettings Settings { get; set; }
        public ShellDescriptor Descriptor { get; set; }

        public IDictionary<Type, DependencyBlueprint> Dependencies { get; set; }

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public ShellSettings Settings { get; set; }
        public ShellDescriptor Descriptor { get; set; }

        public IDictionary<Type, FeatureEntry> Dependencies { get; set; }

        public IEnumerable<TypeInfo> Types
        {
            get
            {
                return Dependencies.Keys.Select(t => t.GetTypeInfo());
            }
        }
    }
}
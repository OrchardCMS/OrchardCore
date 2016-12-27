using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Glimpse.Internal;
using Microsoft.Extensions.DependencyModel;
using Orchard.Environment.Shell.Builders.Models;

namespace Orchard.Glimpse
{
    public class OrchardAssemblyProvider : IAssemblyProvider
    {
        private static HashSet<string> ReferenceAssemblies { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Glimpse.Common",
            "Glimpse.Server",
            "Glimpse.Agent.AspNet",
            "Glimpse.Agent.AspNet.Mvc"
        };

        private readonly ShellBlueprint _shellBlueprint;

        public OrchardAssemblyProvider(ShellBlueprint shellBlueprint)
        {
            _shellBlueprint = shellBlueprint;
        }

        public IEnumerable<Assembly> GetCandidateAssemblies(string coreLibrary)
        {
            return GetCandidateLibraries(DependencyContext.Default)
                .Select(Assembly.Load)
                .Concat(_shellBlueprint.Dependencies.Select(x => x.Type.GetTypeInfo().Assembly))
                .Distinct();
        }

        // Returns a list of libraries that references the assemblies in <see cref="ReferenceAssemblies"/>.
        // By default it returns all assemblies that reference any of the primary MVC assemblies
        // while ignoring MVC assemblies.
        // Internal for unit testing
        private IEnumerable<AssemblyName> GetCandidateLibraries(DependencyContext dependencyContext)
        {
            if (ReferenceAssemblies == null)
            {
                return Enumerable.Empty<AssemblyName>();
            }

            return ReferenceAssemblies
                .Select(x => new AssemblyName(x));
                //.SelectMany(x => dependencyContext.GetRuntimeAssemblyNames(x))
                //.Distinct()
                //.Concat();
        }
    }
}
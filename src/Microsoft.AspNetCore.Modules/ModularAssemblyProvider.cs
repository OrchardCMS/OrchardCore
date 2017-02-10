using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Orchard.Environment.Shell.Builders.Models;

namespace Microsoft.AspNetCore.Modules
{
    public class ModularAssemblyProvider : IModularAssemblyProvider
    {
        private readonly ShellBlueprint _shellBlueprint;

        public ModularAssemblyProvider(ShellBlueprint shellBlueprint)
        {
            _shellBlueprint = shellBlueprint;
        }

        private IList<Assembly> CachedAssemblies;
        private IList<Assembly> CachedRuntimeAssemblies;

        // Returns a list of assemblies and their dependencies contained in runtimeAssemblies
        public IEnumerable<Assembly> GetAssemblies(IEnumerable<Assembly> runtimeAssemblies, ISet<string> referenceAssemblies)
        {
            if (CachedRuntimeAssemblies == null)
            {
                if (!runtimeAssemblies.Any() || !referenceAssemblies.Any())
                {
                    return Enumerable.Empty<Assembly>();
                }

                var types = GetModularTypes();
                var assemblies = types.Select(x => x.Assembly);

                var runtimeAssembliesByNames = runtimeAssemblies
                    .ToDictionary(a => a.GetName().Name);

                var references = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var assembly in assemblies)
                {
                    if (runtimeAssembliesByNames.ContainsKey(assembly.GetName().Name) &&
                        references.Add(assembly.GetName().Name))
                    {
                        AddReferences(assembly, runtimeAssembliesByNames, references);
                    }
                }

                CachedRuntimeAssemblies = DefaultModularAssemblyDiscoveryProvider
                    .GetCandidateAssemblies(
                        runtimeAssemblies
                        .Where(a => references.Contains(a.GetName().Name)),
                        referenceAssemblies)
                    .ToList();
            }

            return CachedRuntimeAssemblies;
        }

        internal static void AddReferences(Assembly assembly, IDictionary<string, Assembly> runtimeAssemblies, HashSet<string> references)
        {
            foreach (var assemblyName in assembly.GetReferencedAssemblies())
            {
                if (runtimeAssemblies.ContainsKey(assemblyName.Name) &&
                    references.Add(assemblyName.Name))
                {
                    var referencedAssembly = runtimeAssemblies[assemblyName.Name];
                    AddReferences(referencedAssembly, runtimeAssemblies, references);
                }
            }
        }

        public IEnumerable<Assembly> GetAssemblies()
        {
            if (CachedAssemblies == null)
            {
                var types = GetModularTypes();
                var assemblies = types.Select(x => x.Assembly);

                var loadedContextAssemblies = new List<Assembly>();
                var assemblyNames = new HashSet<string>();

                foreach (var assembly in assemblies)
                {
                    var currentAssemblyName =
                        Path.GetFileNameWithoutExtension(assembly.Location);

                    if (assemblyNames.Add(currentAssemblyName))
                    {
                        loadedContextAssemblies.Add(assembly);
                    }
                    loadedContextAssemblies.AddRange(GetAssemblies(assemblyNames, assembly));
                }

                CachedAssemblies = loadedContextAssemblies;
            }

            return CachedAssemblies;
        }

        private static IList<Assembly> GetAssemblies(HashSet<string> assemblyNames, Assembly assembly)
        {
            var loadContext = AssemblyLoadContext.GetLoadContext(assembly);
            var referencedAssemblyNames = assembly.GetReferencedAssemblies()
                .Where(ass => !assemblyNames.Contains(ass.Name));

            var locations = new List<Assembly>();

            foreach (var referencedAssemblyName in referencedAssemblyNames)
            {
                if (assemblyNames.Add(referencedAssemblyName.Name))
                {
                    var referencedAssembly = loadContext
                        .LoadFromAssemblyName(referencedAssemblyName);

                    locations.Add(referencedAssembly);

                    locations.AddRange(GetAssemblies(assemblyNames, referencedAssembly));
                }
            }

            return locations;
        }

        public IEnumerable<TypeInfo> GetModularTypes()
        {
            return _shellBlueprint.Dependencies.Select(x => x.Type.GetTypeInfo());
        }
    }
}

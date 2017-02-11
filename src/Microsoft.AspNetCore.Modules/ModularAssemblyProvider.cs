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

        // Returns a list of assemblies and their dependencies contained in runtimeAssemblies
        public IEnumerable<Assembly> GetAssemblies(IEnumerable<Assembly> runtimeAssemblies, ISet<string> referenceAssemblies)
        {
            if (CachedAssemblies == null)
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

                CachedAssemblies = DefaultModularAssemblyDiscoveryProvider
                    .GetCandidateAssemblies(
                        runtimeAssemblies
                        .Where(a => references.Contains(a.GetName().Name)),
                        referenceAssemblies)
                    .ToList();
            }

            return CachedAssemblies;
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

        public IEnumerable<TypeInfo> GetModularTypes()
        {
            return _shellBlueprint.Dependencies.Select(x => x.Type.GetTypeInfo());
        }
    }
}

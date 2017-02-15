using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public IEnumerable<Assembly> GetAssemblies(ISet<string> referenceAssemblies)
        {
            if (CachedAssemblies == null)
            {
                if (!referenceAssemblies.Any())
                {
                    return Enumerable.Empty<Assembly>();
                }

                var dependencies = new List<Assembly>();
                var references = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var assemblies = _shellBlueprint.Dependencies.Keys
                    .Select(x => x.GetTypeInfo().Assembly)
                    .Distinct();

                foreach (var assembly in assemblies)
                {
                    dependencies.AddRange(AddReferences(assembly, references));
                }

                CachedAssemblies = DefaultAssemblyDiscoveryProvider
                    .GetCandidateAssemblies(dependencies, referenceAssemblies)
                    .ToList();
            }

            return CachedAssemblies;
        }

        internal IEnumerable<Assembly> AddReferences(Assembly assembly, HashSet<string> references)
        {
            if (references.Add(assembly.GetName().Name))
            {
                yield return assembly;
            }

            foreach (var assemblyName in assembly.GetReferencedAssemblies())
            {
                if (references.Add(assemblyName.Name))
                {
                    var referenced = Assembly.Load(assemblyName);
                    AddReferences(referenced, references);
                    yield return referenced;
                }
            }
        }
    }
}

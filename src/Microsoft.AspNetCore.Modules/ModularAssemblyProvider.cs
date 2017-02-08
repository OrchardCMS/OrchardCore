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

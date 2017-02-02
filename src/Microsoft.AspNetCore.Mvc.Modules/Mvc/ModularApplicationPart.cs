using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Extensions;
using Orchard.Environment.Shell.Builders.Models;
using Orchard.Environment.Shell.Descriptor.Models;

namespace Microsoft.AspNetCore.Mvc.Modules.Mvc
{
    /// <summary>
    /// An <see cref="ApplicationPart"/> backed by an <see cref="Assembly"/>.
    /// </summary>
    public class ModularApplicationPart :
        ApplicationPart,
        IApplicationPartTypeProvider,
        ICompilationReferencesProvider
    {
        /// <summary>
        /// Initalizes a new <see cref="AssemblyPart"/> instance.
        /// </summary>
        /// <param name="assembly"></param>
        public ModularApplicationPart(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor == null)
            {
                throw new ArgumentNullException(nameof(httpContextAccessor));
            }

            HttpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets the <see cref="Assembly"/> of the <see cref="ApplicationPart"/>.
        /// </summary>
        public IHttpContextAccessor HttpContextAccessor { get; }

        public override string Name
        {
            get
            {
                return typeof(ModularApplicationPart).GetTypeInfo().Assembly.GetName().Name;
            }
        }

        private Lazy<IEnumerable<Assembly>> InitializedAssemblies => new Lazy<IEnumerable<Assembly>>(() =>
        {
            var serviceProvider =
                HttpContextAccessor.HttpContext.RequestServices;

            var assemblies = GetModularAssemblies(serviceProvider)
                .Select(x => x.Assembly);

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

            return loadedContextAssemblies;
        });

        /// <inheritdoc />
        public IEnumerable<TypeInfo> Types
        {
            get
            {
                return InitializedAssemblies.Value.SelectMany(x => x.DefinedTypes);
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> GetReferencePaths()
        {
            return InitializedAssemblies.Value.Select(x => x.Location);
        }

        private static IList<string> GetAssemblyLocations(HashSet<string> assemblyNames, Assembly assembly)
        {
            var loadContext = AssemblyLoadContext.GetLoadContext(assembly);
            var referencedAssemblyNames = assembly.GetReferencedAssemblies()
                .Where(ass => !assemblyNames.Contains(ass.Name));

            var locations = new List<string>();

            foreach (var referencedAssemblyName in referencedAssemblyNames)
            {
                if (assemblyNames.Add(referencedAssemblyName.Name))
                {
                    var referencedAssembly = loadContext
                        .LoadFromAssemblyName(referencedAssemblyName);

                    locations.Add(referencedAssembly.Location);

                    locations.AddRange(GetAssemblyLocations(assemblyNames, referencedAssembly));
                }
            }

            return locations;
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

        public static IEnumerable<TypeInfo> GetModularAssemblies(IServiceProvider services)
        {
            var blueprint = services
                .GetRequiredService<ShellBlueprint>();

            return blueprint.Dependencies.Select(x => x.Type.GetTypeInfo());
        }
    }
} 
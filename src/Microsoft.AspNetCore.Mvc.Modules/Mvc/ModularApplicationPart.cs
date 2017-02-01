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

            return GetModularAssemblies(serviceProvider);
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
            var assemblies = InitializedAssemblies.Value;

            var loadedContextAssemblies = new List<string>();
            var assemblyNames = new HashSet<string>();

            foreach (var assembly in assemblies)
            {
                var currentAssemblyName =
                    Path.GetFileNameWithoutExtension(assembly.Location);

                if (assemblyNames.Add(currentAssemblyName))
                {
                    loadedContextAssemblies.Add(assembly.Location);
                }
                loadedContextAssemblies.AddRange(GetAssemblyLocations(assemblyNames, assembly));
            }

            return loadedContextAssemblies;
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

        public static IEnumerable<Assembly> GetModularAssemblies(IServiceProvider services)
        {
            var extensionManager = services.GetRequiredService<IExtensionManager>();
            var descriptor = services
                .GetRequiredService<ShellDescriptor>();

            var features = extensionManager
                .GetFeatures().Where(f => descriptor.Features.Any(sf => sf.Id == f.Id));

            var extensionsToLoad = features
                .Select(feature => feature.Extension)
                .Distinct();

            var bagOfAssemblies = new List<Assembly>();
            foreach (var extension in extensionsToLoad)
            {
                var extensionEntry = extensionManager
                    .LoadExtensionAsync(extension)
                    .GetAwaiter()
                    .GetResult();

                if (!extensionEntry.IsError)
                {
                    bagOfAssemblies.Add(extensionEntry.Assembly);
                }
            }

            return bagOfAssemblies;
        }
    }
} 
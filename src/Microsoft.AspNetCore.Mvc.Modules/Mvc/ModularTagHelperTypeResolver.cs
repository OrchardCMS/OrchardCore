using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Orchard.Environment.Extensions;
using Orchard.Environment.Shell.Descriptor.Models;

namespace Microsoft.AspNetCore.Mvc.Modules.Mvc
{
    /// <summary>
    /// Class that locates valid <see cref="ITagHelper"/>s within an assembly.
    /// </summary>
    public class ModularTagHelperTypeResolver : ITagHelperTypeResolver
    {
        private static readonly TypeInfo ITagHelperTypeInfo = typeof(ITagHelper).GetTypeInfo();

        private readonly ShellDescriptor _shellDescriptor;
        private readonly IExtensionManager _extensionManager;


        private readonly TagHelperFeature _feature;

        public ModularTagHelperTypeResolver(
            ApplicationPartManager manager,
            ShellDescriptor shellDescriptor,
            IExtensionManager extensionManager)
        {
            _shellDescriptor = shellDescriptor;
            _extensionManager = extensionManager;

            _feature = new TagHelperFeature();
            manager.PopulateFeature(_feature);
        }

        public IHttpContextAccessor HttpContextAccessor { get; }

        /// <inheritdoc />
        public IEnumerable<Type> Resolve(
            string name,
            SourceLocation documentLocation,
            ErrorSink errorSink)
        {
            if (errorSink == null)
            {
                throw new ArgumentNullException(nameof(errorSink));
            }

            if (string.IsNullOrEmpty(name))
            {
                var errorLength = name == null ? 1 : Math.Max(name.Length, 1);
                errorSink.OnError(
                    documentLocation,
                    "Tag helper directive assembly name cannot be null or empty.",
                    errorLength);

                return Type.EmptyTypes;
            }

            var assemblyName = new AssemblyName(name);

            IEnumerable<TypeInfo> libraryTypes;
            try
            {
                libraryTypes = GetExportedTypes(assemblyName);
            }
            catch (Exception ex)
            {
                errorSink.OnError(
                    documentLocation,
                    $"Cannot resolve TagHelper containing assembly '{assemblyName.Name}'. Error: {ex.Message}",
                    name.Length);

                return Type.EmptyTypes;
            }

            if (!libraryTypes.Any())
            {
                Console.WriteLine();
            }

            return libraryTypes.Where(IsTagHelper).Select(t => t.AsType());
        }

        /// <summary>
        /// Returns all exported types from the given <paramref name="assemblyName"/>
        /// </summary>
        /// <param name="assemblyName">The <see cref="AssemblyName"/> to get <see cref="TypeInfo"/>s from.</param>
        /// <returns>
        /// An <see cref="IEnumerable{TypeInfo}"/> of types exported from the given <paramref name="assemblyName"/>.
        /// </returns>
        protected virtual IEnumerable<TypeInfo> GetExportedTypes(AssemblyName assemblyName)
        {
            if (assemblyName == null)
            {
                throw new ArgumentNullException(nameof(assemblyName));
            }

            var results = new List<TypeInfo>();
            for (var i = 0; i < _feature.TagHelpers.Count; i++)
            {
                var tagHelperAssemblyName = _feature.TagHelpers[i].Assembly.GetName();

                if (AssemblyNameComparer.OrdinalIgnoreCase.Equals(tagHelperAssemblyName, assemblyName))
                {
                    results.Add(_feature.TagHelpers[i]);
                }
            }

            return results;
        }

        /// <summary>
        /// Indicates if a <see cref="TypeInfo"/> should be treated as a tag helper.
        /// </summary>
        /// <param name="typeInfo">The <see cref="TypeInfo"/> to inspect.</param>
        /// <returns><c>true</c> if <paramref name="typeInfo"/> should be treated as a tag helper; 
        /// <c>false</c> otherwise</returns>
        protected virtual bool IsTagHelper(TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            return TagHelperConventions.IsTagHelper(typeInfo);
        }

        private class AssemblyNameComparer : IEqualityComparer<AssemblyName>
        {
            public static readonly IEqualityComparer<AssemblyName> OrdinalIgnoreCase = new AssemblyNameComparer();

            private AssemblyNameComparer()
            {
            }

            public bool Equals(AssemblyName x, AssemblyName y)
            {
                // Ignore case because that's what Assembly.Load does.
                return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.CultureName ?? string.Empty, y.CultureName ?? string.Empty, StringComparison.Ordinal);
            }

            public int GetHashCode(AssemblyName obj)
            {
                var hashCode = 0;
                if (obj.Name != null)
                {
                    hashCode ^= obj.Name.GetHashCode();
                }

                hashCode ^= (obj.CultureName ?? string.Empty).GetHashCode();
                return hashCode;
            }
        }

        private Lazy<IEnumerable<Assembly>> InitializedAssemblies => new Lazy<IEnumerable<Assembly>>(() =>
        {
            return GetModularAssemblies();
        });

        /// <inheritdoc />
        public IEnumerable<Assembly> GetReferencedAssemblies()
        {
            var assemblies = InitializedAssemblies.Value;
                
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

        public IEnumerable<Assembly> GetModularAssemblies()
        {
            var features = _extensionManager
                .GetFeatures().Where(f => _shellDescriptor.Features.Any(sf => sf.Id == f.Id));

            var extensionsToLoad = features
                .Select(feature => feature.Extension)
                .Distinct();

            var bagOfAssemblies = new List<Assembly>();
            foreach (var extension in extensionsToLoad)
            {
                var extensionEntry = _extensionManager
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

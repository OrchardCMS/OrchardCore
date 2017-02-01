//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Reflection.PortableExecutable;
//using System.Runtime.Loader;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc.ApplicationParts;
//using Microsoft.AspNetCore.Mvc.Razor.Compilation;
//using Microsoft.CodeAnalysis;
//using Microsoft.Extensions.DependencyInjection;
//using Orchard.Environment.Extensions;
//using Orchard.Environment.Shell;

//namespace Microsoft.AspNetCore.Mvc.Modules.Mvc
//{
//    public class ModularFeatureProvider
//        : IApplicationFeatureProvider<MetadataReferenceFeature>
//    {
//        private readonly IHttpContextAccessor _httpContextAccessor;
//        public ModularFeatureProvider(IHttpContextAccessor httpContextAccessor)
//        {
//            _httpContextAccessor = httpContextAccessor;
//        }

//        public void PopulateFeature(IEnumerable<ApplicationPart> parts, MetadataReferenceFeature feature)
//        {
//            if (feature == null)
//            {
//                throw new ArgumentNullException(nameof(feature));
//            }

//            var extensionManager = _httpContextAccessor
//                .HttpContext
//                .RequestServices
//                .GetRequiredService<IExtensionManager>();

//            var featureManager = _httpContextAccessor
//                .HttpContext
//                .RequestServices
//                .GetRequiredService<IShellFeaturesManager>();

//            var enabledAssemblies =
//                featureManager
//                .GetEnabledFeaturesAsync()
//                .GetAwaiter()
//                .GetResult()
//                .Select(x => x.Extension);

//            var bagOfAssemblies = new List<Assembly>();
//            foreach (var extension in enabledAssemblies)
//            {
//                var extensionEntry = extensionManager
//                    .LoadExtensionAsync(extension)
//                    .GetAwaiter()
//                    .GetResult();

//                if (!extensionEntry.IsError)
//                {
//                    bagOfAssemblies.Add(extensionEntry.Assembly);
//                }
//            }


//            var libraryPaths = new List<string>();
//            var assemblyNames = new HashSet<string>();

//            foreach (var assembly in bagOfAssemblies)
//            {
//                var currentAssemblyName =
//                    Path.GetFileNameWithoutExtension(assembly.Location);

//                if (assemblyNames.Add(currentAssemblyName))
//                {
//                    libraryPaths.Add(assembly.Location);
//                }
//                libraryPaths.AddRange(GetAssemblyLocations(assemblyNames, assembly));
//            }

//            var orderedLibraryPaths = libraryPaths.OrderBy(x => x).ToArray();

//            foreach (var location in orderedLibraryPaths)
//            {
//                var metadataReference = CreateMetadataReference(location);
//                feature.MetadataReferences.Add(metadataReference);
//            }
//        }

//        private static MetadataReference CreateMetadataReference(string path)
//        {
//            using (var stream = File.OpenRead(path))
//            {
//                var moduleMetadata = ModuleMetadata.CreateFromStream(stream, PEStreamOptions.PrefetchMetadata);
//                var assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);

//                return assemblyMetadata.GetReference(filePath: path);
//            }
//        }

//        private static IList<string> GetAssemblyLocations(HashSet<string> assemblyNames, Assembly assembly)
//        {
//            var loadContext = AssemblyLoadContext.GetLoadContext(assembly);
//            var referencedAssemblyNames = assembly.GetReferencedAssemblies()
//                .Where(ass => !assemblyNames.Contains(ass.Name));

//            var locations = new List<string>();

//            foreach (var referencedAssemblyName in referencedAssemblyNames)
//            {
//                if (assemblyNames.Add(referencedAssemblyName.Name))
//                {
//                    var referencedAssembly = loadContext
//                        .LoadFromAssemblyName(referencedAssemblyName);

//                    locations.Add(referencedAssembly.Location);

//                    locations.AddRange(GetAssemblyLocations(assemblyNames, referencedAssembly));
//                }
//            }

//            return locations;
//        }
//    }
//}

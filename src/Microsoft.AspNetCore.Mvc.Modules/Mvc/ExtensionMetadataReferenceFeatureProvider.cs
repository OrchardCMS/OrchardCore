using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.Mvc.Modules.Mvc
{
    public class ExtensionMetadataReferenceFeatureProvider
        : IApplicationFeatureProvider<MetadataReferenceFeature>
    {
        private readonly string[] _metadataReferencePaths;
        public ExtensionMetadataReferenceFeatureProvider(string[] metadataReferencePaths)
        {
            _metadataReferencePaths = metadataReferencePaths;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, MetadataReferenceFeature feature)
        {
            if (feature == null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            var libraryPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var referencePaths = _metadataReferencePaths;
            foreach (var path in referencePaths)
            {
                if (libraryPaths.Add(path))
                {
                    var metadataReference = CreateMetadataReference(path);
                    feature.MetadataReferences.Add(metadataReference);
                }
            }

            //foreach (var providerPart in parts.OfType<ICompilationReferencesProvider>())
            //{
            //    var referencePathsa = providerPart.GetReferencePaths();
            //    foreach (var path in referencePathsa)
            //    {
            //        if (libraryPaths.Add(path))
            //        {
            //            var metadataReference = CreateMetadataReference(path);
            //            feature.MetadataReferences.Add(metadataReference);
            //        }
            //    }
            //}
        }

        private static MetadataReference CreateMetadataReference(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var moduleMetadata = ModuleMetadata.CreateFromStream(stream, PEStreamOptions.PrefetchMetadata);
                var assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);

                return assemblyMetadata.GetReference(filePath: path);
            }
        }
    }
}

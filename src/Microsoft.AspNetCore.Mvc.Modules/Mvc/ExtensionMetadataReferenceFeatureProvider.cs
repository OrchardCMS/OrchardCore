using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Orchard.Environment.Extensions;

namespace Microsoft.AspNetCore.Mvc.Modules.Mvc
{
    public class ExtensionMetadataReferenceFeatureProvider
        : IApplicationFeatureProvider<MetadataReferenceFeature>
    {
        private readonly IExtensionLibraryService _applicationServices;

        public ExtensionMetadataReferenceFeatureProvider(
            IServiceProvider serviceProvider) {
            _applicationServices = (IExtensionLibraryService)serviceProvider.GetService(typeof(IExtensionLibraryService));
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, 
            MetadataReferenceFeature feature)
        {
            foreach (var reference in _applicationServices.MetadataReferences())
            {
                feature.MetadataReferences.Add(reference);
            }
        }
    }
}

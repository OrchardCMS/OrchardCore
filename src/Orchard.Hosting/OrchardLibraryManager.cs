using System.Collections.Generic;
using System.Linq;
using Microsoft.Dnx.Compilation;
using Orchard.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;

namespace Orchard.Hosting
{
    public class OrchardLibraryManager : IOrchardLibraryManager
    {
        private readonly ILibraryManager _libraryManager;

        public OrchardLibraryManager(ILibraryManager libraryManager)
        {
            _libraryManager = libraryManager;

            AdditionalLibraries = new List<Library>();
            MetadataReferences = new List<IMetadataReference>();
        }

        private IList<Library> AdditionalLibraries { get; }
        private IList<IMetadataReference> MetadataReferences { get; }

        public void AddLibrary(Library library)
        {
            if (AdditionalLibraries.Any(lib => lib.Name == library.Name))
                return;

            AdditionalLibraries.Add(library);
        }

        public void AddMetadataReference(IMetadataReference metadataReference)
        {
            if (MetadataReferences.Any(x => x.Name == metadataReference.Name))
                return;

            MetadataReferences.Add(metadataReference);
        }

        public IMetadataReference GetMetadataReference(string name)
        {
            return MetadataReferences.SingleOrDefault(x => x.Name == name);
        }

        public IEnumerable<IMetadataReference> GetAllMetadataReferences()
        {
            return MetadataReferences;
        }

        public IEnumerable<Library> GetReferencingLibraries(string name)
        {
            return _libraryManager.GetReferencingLibraries(name)
                .Union(AdditionalLibraries
                    .Where(x => x.Dependencies.FirstOrDefault(o => o == name) != null));
        }

        public Library GetLibrary(string name)
        {
            var info = _libraryManager.GetLibrary(name);
            if (info != null)
                return info;

            return AdditionalLibraries.SingleOrDefault(x => x.Name == name);
        }

        public IEnumerable<Library> GetLibraries()
        {
            return _libraryManager
                .GetLibraries()
                .Concat(AdditionalLibraries);
        }
    }
}
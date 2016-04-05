using Microsoft.Extensions.PlatformAbstractions;
using System.Collections.Generic;
using Microsoft.Extensions.CompilationAbstractions;

namespace Orchard.DependencyInjection
{
    public interface IOrchardLibraryManager : ILibraryManager
    {
        void AddLibrary(Library library);
        void AddMetadataReference(IMetadataReference metadataReference);
        IMetadataReference GetMetadataReference(string name);
        IEnumerable<IMetadataReference> GetAllMetadataReferences();
    }
}
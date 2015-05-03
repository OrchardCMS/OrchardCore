using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Compilation;

namespace OrchardVNext.Environment.Extensions.Loaders {
    public interface IOrchardLibraryManager : ILibraryManager {
        IDictionary<string, IMetadataReference> MetadataReferences { get; }
        void AddAdditionalRegistrations(IList<LibraryDescription> additionalRegistrations);
        void AddAdditionalLibraryExportRegistrations(string name, ILibraryExport additionalRegistration);
        void AddMetadataReference(string name, IMetadataReference metadataReference);
    }

    public class OrchardLibraryManager : IOrchardLibraryManager {
        private readonly ILibraryManager _libraryManager;

        public OrchardLibraryManager(ILibraryManager libraryManager) {
            _libraryManager = libraryManager;

            AdditionalRegistrations = new List<LibraryDescription>();
            AdditionalLibraryExportRegistrations = new Dictionary<string, ILibraryExport>();
            MetadataReferences = new Dictionary<string, IMetadataReference>();
        }

        public IList<LibraryDescription> AdditionalRegistrations { get; }
        public IDictionary<string, ILibraryExport> AdditionalLibraryExportRegistrations { get; }
        public IDictionary<string, IMetadataReference> MetadataReferences { get; }

        public void AddAdditionalRegistrations(IList<LibraryDescription> additionalRegistrations) {
            foreach (var registration in additionalRegistrations) {
                if (AdditionalRegistrations.All(x => x.Identity.Name != registration.Identity.Name))
                    AdditionalRegistrations.Add(registration);
            }
        }

        public void AddAdditionalLibraryExportRegistrations(string name, ILibraryExport additionalRegistration) {
            AdditionalLibraryExportRegistrations[name] = additionalRegistration;
        }

        public void AddMetadataReference(string name, IMetadataReference metadataReference) {
            MetadataReferences[name] = metadataReference;
        }

        public ILibraryExport GetAllExports(string name) {
            return _libraryManager.GetAllExports(name);
        }

        public ILibraryExport GetAllExports(string name, string aspect) {
            return _libraryManager.GetAllExports(name, aspect);
        }

        public IEnumerable<ILibraryInformation> GetLibraries() {
            return _libraryManager.GetLibraries();
        }

        public IEnumerable<ILibraryInformation> GetLibraries(string aspect) {
            return _libraryManager.GetLibraries(aspect);
        }

        public ILibraryExport GetLibraryExport(string name) {
            return _libraryManager.GetLibraryExport(name);
        }

        public ILibraryExport GetLibraryExport(string name, string aspect) {
            var export =  _libraryManager.GetLibraryExport(name, aspect);
            if (export != null)
                return export;

            return AdditionalLibraryExportRegistrations[name];
        }

        public ILibraryInformation GetLibraryInformation(string name) {
            var info = _libraryManager.GetLibraryInformation(name);
            if (info != null)
                return info;

            var lib = AdditionalRegistrations.SingleOrDefault(x => x.Identity.Name == name);
            if (lib != null)
                return new LibraryInformation(lib);

            return null;
        }

        public ILibraryInformation GetLibraryInformation(string name, string aspect) {
            var info = _libraryManager.GetLibraryInformation(name, aspect);
            if (info != null)
                return info;

            var lib = AdditionalRegistrations.SingleOrDefault(x => x.Identity.Name == name);
            if (lib != null)
                return new LibraryInformation(lib);

            return null;
        }

        public IEnumerable<ILibraryInformation> GetReferencingLibraries(string name) {
            return _libraryManager.GetReferencingLibraries(name)
                .Union(AdditionalRegistrations
                    .Where(x => x.Dependencies.FirstOrDefault(o => o.Name == name) != null)
                    .Select(x => new LibraryInformation(x)));
        }

        public IEnumerable<ILibraryInformation> GetReferencingLibraries(string name, string aspect) {
            return _libraryManager.GetReferencingLibraries(name, aspect);
        }
    }
}
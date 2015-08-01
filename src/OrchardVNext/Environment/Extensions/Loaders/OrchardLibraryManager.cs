using System.Collections.Generic;
using System.Linq;
using Microsoft.Dnx.Compilation;
using Microsoft.Dnx.Runtime;

namespace OrchardVNext.Environment.Extensions.Loaders {
    public interface IOrchardLibraryManager : ILibraryManager, ILibraryExporter {
        IDictionary<string, IMetadataReference> MetadataReferences { get; }
        void AddAdditionalRegistrations(IList<LibraryDescription> additionalRegistrations);
        void AddAdditionalLibraryExportRegistrations(string name, LibraryExport additionalRegistration);
        void AddMetadataReference(string name, IMetadataReference metadataReference);
    }

    public class OrchardLibraryManager : IOrchardLibraryManager {
        private readonly ILibraryManager _libraryManager;
        private readonly ILibraryExporter _libraryExporter;

        public OrchardLibraryManager(ILibraryManager libraryManager, ILibraryExporter libraryExporter) {
            _libraryManager = libraryManager;
            _libraryExporter = libraryExporter;

            AdditionalRegistrations = new List<LibraryDescription>();
            AdditionalLibraryExportRegistrations = new Dictionary<string, LibraryExport>();
            MetadataReferences = new Dictionary<string, IMetadataReference>();
        }

        public IList<LibraryDescription> AdditionalRegistrations { get; }
        public IDictionary<string, LibraryExport> AdditionalLibraryExportRegistrations { get; }
        public IDictionary<string, IMetadataReference> MetadataReferences { get; }

        public void AddAdditionalRegistrations(IList<LibraryDescription> additionalRegistrations) {
            foreach (var registration in additionalRegistrations) {
                if (AdditionalRegistrations.All(x => x.Identity.Name != registration.Identity.Name))
                    AdditionalRegistrations.Add(registration);
            }
        }

        public void AddAdditionalLibraryExportRegistrations(string name, LibraryExport additionalRegistration) {
            AdditionalLibraryExportRegistrations[name] = additionalRegistration;
        }

        public void AddMetadataReference(string name, IMetadataReference metadataReference) {
            MetadataReferences[name] = metadataReference;
        }


        public IEnumerable<Library> GetReferencingLibraries(string name) {
            return _libraryManager.GetReferencingLibraries(name)
                .Union(AdditionalRegistrations
                    .Where(x => x.Dependencies.FirstOrDefault(o => o.Name == name) != null)
                    .Select(x => x.ToLibrary()));
        }

        public Library GetLibraryInformation(string name) {
            var info = _libraryManager.GetLibraryInformation(name);
            if (info != null)
                return info;

            var lib = AdditionalRegistrations.SingleOrDefault(x => x.Identity.Name == name);
            if (lib != null)
                return lib.ToLibrary();

            return null;
        }

        public IEnumerable<Library> GetLibraries() {
            return _libraryManager
                .GetLibraries()
                .Concat(AdditionalRegistrations.Select(x => x.ToLibrary()));
        }

        public LibraryExport GetLibraryExport(string name) {
            var export = _libraryExporter.GetLibraryExport(name);
            if (export != null)
                return export;

            return AdditionalLibraryExportRegistrations[name];
        }

        public LibraryExport GetLibraryExport(string name, string aspect) {
            var export = _libraryExporter.GetLibraryExport(name);
            if (export != null)
                return export;

            return AdditionalLibraryExportRegistrations[name];
        }

        public LibraryExport GetAllExports(string name) {
            var internalExports = _libraryExporter.GetAllExports(name);

            return new LibraryExport(
                internalExports.MetadataReferences.AsEnumerable().Concat(AdditionalLibraryExportRegistrations.SelectMany(x => x.Value.MetadataReferences)).ToList(),
                internalExports.SourceReferences.AsEnumerable().Concat(AdditionalLibraryExportRegistrations.SelectMany(x => x.Value.SourceReferences)).ToList()
                );
        }

        public LibraryExport GetAllExports(string name, string aspect) {
            var internalExports = _libraryExporter.GetAllExports(name, aspect);

            return new LibraryExport(
                internalExports.MetadataReferences.AsEnumerable().Concat(AdditionalLibraryExportRegistrations.SelectMany(x => x.Value.MetadataReferences)).ToList(),
                internalExports.SourceReferences.AsEnumerable().Concat(AdditionalLibraryExportRegistrations.SelectMany(x => x.Value.SourceReferences)).ToList()
                );
        }
    }
}
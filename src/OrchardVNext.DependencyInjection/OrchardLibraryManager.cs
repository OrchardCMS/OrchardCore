using System.Collections.Generic;
using System.Linq;
using Microsoft.Dnx.Compilation;
using Microsoft.Dnx.Runtime;

namespace OrchardVNext.DependencyInjection {
    public interface IOrchardLibraryManager : ILibraryManager, ILibraryExporter {
        IDictionary<string, IMetadataReference> MetadataReferences { get; }
        void AddLibrary(Library library);
        void AddAdditionalLibraryExportRegistrations(string name, LibraryExport additionalRegistration);
        void AddMetadataReference(string name, IMetadataReference metadataReference);
    }

    public class OrchardLibraryManager : IOrchardLibraryManager {
        private readonly ILibraryManager _libraryManager;
        private readonly ILibraryExporter _libraryExporter;

        public OrchardLibraryManager(ILibraryManager libraryManager, ILibraryExporter libraryExporter) {
            _libraryManager = libraryManager;
            _libraryExporter = libraryExporter;

            AdditionalLibraries = new List<Library>();
            AdditionalLibraryExportRegistrations = new Dictionary<string, LibraryExport>();
            MetadataReferences = new Dictionary<string, IMetadataReference>();
        }

        private IList<Library> AdditionalLibraries { get; }
        public IDictionary<string, LibraryExport> AdditionalLibraryExportRegistrations { get; }
        public IDictionary<string, IMetadataReference> MetadataReferences { get; }

        public void AddLibrary(Library library) {
            if (AdditionalLibraries.Any(lib => lib.Name == library.Name))
                return;

            AdditionalLibraries.Add(library);
        }

        public void AddAdditionalLibraryExportRegistrations(string name, LibraryExport additionalRegistration) {
            AdditionalLibraryExportRegistrations[name] = additionalRegistration;
        }

        public void AddMetadataReference(string name, IMetadataReference metadataReference) {
            MetadataReferences[name] = metadataReference;
        }


        public IEnumerable<Library> GetReferencingLibraries(string name) {
            return _libraryManager.GetReferencingLibraries(name)
                .Union(AdditionalLibraries
                    .Where(x => x.Dependencies.FirstOrDefault(o => o == name) != null));
        }

        public Library GetLibrary(string name) {
            var info = _libraryManager.GetLibrary(name);
            if (info != null)
                return info;

            return AdditionalLibraries.SingleOrDefault(x => x.Name == name);
        }

        public IEnumerable<Library> GetLibraries() {
            return _libraryManager
                .GetLibraries()
                .Concat(AdditionalLibraries);
        }

        public LibraryExport GetExport(string name) {
            var export = _libraryExporter.GetExport(name);
            if (export != null)
                return export;

            return AdditionalLibraryExportRegistrations[name];
        }

        public LibraryExport GetExport(string name, string aspect) {
            var export = _libraryExporter.GetExport(name, aspect);
            if (export != null)
                return export;

            return AdditionalLibraryExportRegistrations[name];
        }

        public LibraryExport GetAllExports(string name) {
            var internalExports = _libraryExporter.GetAllExports(name);

            return new LibraryExportWrapper(internalExports,
                AdditionalLibraryExportRegistrations.Select(x => x.Value).ToList());
        }

        public LibraryExport GetAllExports(string name, string aspect) {
            var internalExports = _libraryExporter.GetAllExports(name, aspect);

            return new LibraryExportWrapper(internalExports,
                AdditionalLibraryExportRegistrations.Select(x => x.Value).ToList());
        }

        private class LibraryExportWrapper : LibraryExport {
            public LibraryExportWrapper(
                LibraryExport internalExports, IReadOnlyList<LibraryExport> additionalExports) 
                : base(JoinMetadataReferences(internalExports, additionalExports),
                      JoinSourceReferences(internalExports, additionalExports)) {
            }

            private static IList<ISourceReference> JoinSourceReferences(
                LibraryExport internalExports,
                IReadOnlyList<LibraryExport> additionalExports) {
                return internalExports
                    .SourceReferences
                    .AsEnumerable()
                    .Concat(
                        additionalExports.SelectMany(x => x.SourceReferences)).ToList();
            }

            private static IList<IMetadataReference> JoinMetadataReferences(
                LibraryExport internalExports, 
                IReadOnlyList<LibraryExport> additionalExports) {
                return internalExports
                    .MetadataReferences
                    .AsEnumerable()
                    .Concat(
                        additionalExports.SelectMany(x => x.MetadataReferences)).ToList();
            }
        }
    }
}
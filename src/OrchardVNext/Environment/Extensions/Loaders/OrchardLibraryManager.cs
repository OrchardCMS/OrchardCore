using Microsoft.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace OrchardVNext.Environment.Extensions.Loaders {
    public interface IOrchardLibraryManager : ILibraryManager {
        void AddAdditionalRegistrations(IList<LibraryDescription> additionalRegistrations);
    }

    public class OrchardLibraryManager : IOrchardLibraryManager {
        private readonly ILibraryManager _libraryManager;

        public OrchardLibraryManager(ILibraryManager libraryManager) {
            _libraryManager = libraryManager;

            AdditionalRegistrations = new List<LibraryDescription>();
        }

        private IList<LibraryDescription> AdditionalRegistrations { get; }

        public void AddAdditionalRegistrations(IList<LibraryDescription> additionalRegistrations) {
            foreach (var registration in additionalRegistrations) {
                if (!AdditionalRegistrations.Any(x => x.Identity.Name == registration.Identity.Name))
                    AdditionalRegistrations.Add(registration);
            }
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
            return _libraryManager.GetLibraryExport(name, aspect);
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
                .Union(AdditionalRegistrations.Select(x => new LibraryInformation(x)));
        }

        public IEnumerable<ILibraryInformation> GetReferencingLibraries(string name, string aspect) {
            return _libraryManager.GetReferencingLibraries(name, aspect);
        }
    }
}